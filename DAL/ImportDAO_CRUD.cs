﻿using System;
using System.Collections.Generic;
using System.Xml.Linq;
using DTO;
using Org.BouncyCastle.Utilities.Collections;

namespace DAL
{
    public  partial class ImportDAO : DBConnection
    {
        private static readonly string dbTableName = "import_invoice";
        private static readonly string dbViewName = "review_import_invoice";


        private static ImportInvoice ConvertToDTO(List<object> row)
        {
            try 
            {
                Int16.TryParse(row[0].ToString(), out var id);
                var code = row[1].ToString();
                DateTime.TryParse(row[2].ToString(), out var dateInit);
                Int16.TryParse(row[3].ToString(), out var staffId);
                var staff = StaffDAO.Select(staffId);
                string staffNickname = staff != null ? staff.Nickname : "";
                Int16.TryParse(row[4].ToString(), out var supplierId);
                var supplier = SupplierDAO.Select(supplierId);
                string supplierName = supplier != null ? supplier.Name : "";
                Double.TryParse(row[5].ToString(), out var totalAmount);

                return new ImportInvoice(id, code)
                {
                    DateInit = dateInit,
                    StaffId = staffId,
                    StaffNickName = staffNickname,
                    SupplierId = supplierId,
                    SupplierName = supplierName,
                    TotalAmount = totalAmount
                };
            }
            catch { return null; }
        }

        public static List<ImportInvoice> SelectAllForm()
        {
            string sql = string.Format("SELECT * FROM {0}", dbViewName);
            var table = ExecuteReader(sql);
            var list = new List<ImportInvoice>();
            foreach (var row in table)
            {
                list.Add(ConvertToDTO(row));
            }
            return list;
        }

        public static ImportInvoice SelectForm(string code)
        {
            string sql = string.Format("SELECT * FROM {0} WHERE code = '{1}' LIMIT 1", dbViewName, code);
            var table = ExecuteReader(sql);
            if (table.Count != 0)
            {
                var rs = ConvertToDTO(table[0]);
                rs.Details = SelectDetails(rs.Id);
                return rs;
            }
            return null;
        }

        public static List<ImportDetail> SelectDetails(int formId)
        {
            string sql = string.Format(
                "SELECT barcode,name,mfg_date,exp_date,unit,number,import_price " +
                "FROM import_detail WHERE import_invoice_id={0}", formId);
            var table = ExecuteReader(sql);
            var list = new List<ImportDetail>();
            foreach (var row in table)
            {
                DateTime.TryParse(row[2].ToString(), out var mfg);
                DateTime.TryParse(row[3].ToString(), out var exp);
                ImportDetail detail = new ImportDetail()
                {
                    Barcode = row[0].ToString(),
                    Name = row[1].ToString(),
                    MFGDate = mfg,
                    EXPDate = exp,
                    Unit = row[4].ToString(),
                    Number = Convert.ToInt16(row[5]),
                    ImportPrice = Convert.ToDouble(row[6])
                };

                list.Add(detail);
            }
            return list;
        }

        public static bool Insert(ImportInvoice e)
        {
            if (Open())
            {
                var trans = conn.BeginTransaction();
                var cmd = conn.CreateCommand();
                cmd.Transaction = trans;

                try
                {
                    cmd.CommandText =
                        "INSERT INTO import_invoice(staff_id,supplier_id) " +
                        "VALUES (@staff_id,@supplier_id)";
                    cmd.Parameters.AddWithValue("@staff_id", e.StaffId);
                    cmd.Parameters.AddWithValue("@supplier_id", e.SupplierId);
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();

                    cmd.CommandText =
                        "SELECT auto_increment FROM information_schema.tables " +
                        "WHERE table_name='import_invoice'";
                    int id = Convert.ToInt16(cmd.ExecuteScalar().ToString()) - 1;

                    foreach (var i in e.Details)
                    {
                        int pid = ProductDAO.GetProductId(i.Barcode);
                        if (pid < 0) continue;

                        cmd.CommandText =
                            "INSERT INTO import_detail " +
                            "VALUES (@import_invoice_id,@product_id,@barcode,@name,@mfg_date,@exp_date,@unit,@number,@import_price); " +
                            "UPDATE product SET current_import_price = @import_price WHERE id = @product_id";
                        cmd.Parameters.AddWithValue("@import_invoice_id", id);
                        cmd.Parameters.AddWithValue("@product_id", pid);
                        cmd.Parameters.AddWithValue("@barcode", i.Barcode);
                        cmd.Parameters.AddWithValue("@name", i.Name);
                        cmd.Parameters.AddWithValue("@mfg_date", i.MFGDate.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@exp_date", i.EXPDate.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@unit", i.Unit);
                        cmd.Parameters.AddWithValue("@number", i.Number);
                        cmd.Parameters.AddWithValue("@import_price", i.ImportPrice);
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }

                    trans.Commit();
                    return true;
                }
                catch
                {
                    trans.Rollback();
                    return false;
                }
                finally { Close(); }
            }
            return false;
        }
    }
}
