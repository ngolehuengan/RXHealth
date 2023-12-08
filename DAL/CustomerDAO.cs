﻿using System;
using System.Collections.Generic;
using DTO;

namespace DAL
{
    public class CustomerDAO : DBConnection
    {
        private static readonly string dbTableName = "customer";
        public static int Count
        {
            get
            {
                string sql = string.Format("SELECT COUNT(id) FROM {0}", dbTableName);
                var rs = ExecuteScalar(sql);
                return rs != null ? int.TryParse(rs.ToString(), out int c) ? c - 1 : 0 : 0;
            }
        }
        
        private static Customer ConvertToDTO(List<object> row)
        {
            try
            {
                var rs = new Customer(
                    Convert.ToInt16(row[0]),
                    Convert.ToString(row[1]),
                    Convert.ToString(row[2]),
                    Convert.ToInt16(row[3]) 
                );
                rs.Times = SaleDAO.CountByCustomer(rs.Id);
                return rs;
            }
            catch { return null; }
        }

        public static List<Customer> SelectAll()
        {
            string sql = string.Format("SELECT * FROM {0}", dbTableName);
            var table = ExecuteReader(sql);
            var list = new List<Customer>();
            foreach (var row in table)
            {
                list.Add(ConvertToDTO(row));
            }
            return list;
        }

        public static List<Customer> SearchOnContactNumber(string con)
        {
            string sql = string.Format("SELECT * FROM {0} WHERE contact_number LIKE '%{1}%'", dbTableName, con);
            var table = ExecuteReader(sql);
            var list = new List<Customer>();
            foreach (var row in table)
            {
                list.Add(ConvertToDTO(row));
            }
            return list;
        }

        public static Customer Select(int id)
        {
            string sql = string.Format("SELECT * FROM {0} WHERE id = {1}", dbTableName, id);
            var table = ExecuteReader(sql);
            return table.Count != 0 ? ConvertToDTO(table[table.Count - 1]) : null;
        }

        public static int GetId(string contactNumber)
        {
            string sql = string.Format("SELECT id FROM {0} WHERE contact_number = '{1}' LIMIT 1", dbTableName, contactNumber);
            var rs = ExecuteScalar(sql);
            return rs != null ? int.TryParse(rs.ToString(), out int c) ? c : 1 : 1;
        }

        public static bool ReducePoint(int cusId, int point)
        {
            string sql = string.Format("UPDATE {0} SET total_point = total_point - {2} WHERE id = {1}", dbTableName, cusId, point);
            return ExecuteNonQuery(sql) != -1;
        }

        public static bool Insert(Customer e)
        {
            string sql = string.Format(
                "INSERT INTO {0}(name,contact_number) " +
                "VALUE ('{1}','{2}')", dbTableName, e.Name, e.ContactNumber);
            return ExecuteNonQuery(sql) > 0;
        }

        public static bool Delete(int id)
        {
            string sql = string.Format("DELETE FROM {0} WHERE id = {1}", dbTableName, id);
            return ExecuteNonQuery(sql) > 0;
        }

        public static bool UpdateInformation(Customer e)
        {
            string sql = string.Format("UPDATE {0} SET " +
                "name = '{2}', " +
                "contact_number = '{3}' " +
                "WHERE id = {1}", dbTableName, e.Id, e.Name, e.ContactNumber);
            return ExecuteNonQuery(sql) > 0;
        }
    }
}
