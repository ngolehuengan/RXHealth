﻿using System;
using System.Collections.Generic;
using DTO;

namespace DAL
{
    public class CategoryDAO : DBConnection
    {
        private static readonly string dbTableName = "category";

        private static Category ConvertToDTO(List<object> row)
        {
            try { return new Category(Convert.ToInt16(row[0]), Convert.ToString(row[1])); }
            catch { return null; }
        }

        public static int GetId(string name)
        {
            string sql = string.Format("SELECT id FROM {0} WHERE name = '{1}' LIMIT 1", dbTableName, name);
            var table = ExecuteReader(sql);
            return table.Count != 0 ? Convert.ToInt16(table[0][0]) : 0;
        }
        
        public static List<Category> SelectAll()
        {
            string sql = string.Format("SELECT * FROM {0}", dbTableName);
            var table = ExecuteReader(sql);
            var list = new List<Category>();
            foreach (var row in table) 
            {
                list.Add(ConvertToDTO(row));
            }
            return list;
        }
        
        public static Category Select(int id)
        {
            string sql = string.Format("SELECT * FROM {0} WHERE id = {1}", dbTableName, id);
            var table = ExecuteReader(sql);
            return table.Count != 0 ? ConvertToDTO(table[table.Count - 1]) : null;
        }
        
        public static bool Insert(Category e)
        {
            string sql = string.Format("INSERT INTO {0}(name) VALUE ('{1}')", dbTableName, e.Name);
            return ExecuteNonQuery(sql) > 0;
        }
        
        public static bool Delete(int id)
        {
            string sql = string.Format("DELETE FROM {0} WHERE id = {1}", dbTableName, id);
            return ExecuteNonQuery(sql) > 0;
        }

        public static bool Update(Category e)
        {
            string sql = string.Format("UPDATE {0} SET name = '{2}' WHERE id = {1}", dbTableName, e.Id, e.Name);
            return ExecuteNonQuery(sql) > 0;
        }
    }
}
