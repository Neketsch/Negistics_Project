using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Data.OleDb;
using ExtensionMethods;
namespace Negistics_Project.Common
{
    public static class BusinessLogic
    {
        public static DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            DataTable dt = new DataTable();
            
            using (StreamReader sr = new StreamReader(strFilePath))
            {
                string[] headers = sr.ReadLine().Split(',');
                foreach (string header in headers)
                {
                    dt.Columns.Add(header.Trim());
                }

                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(',');
                    if (rows.Length > 1)
                    {
                        DataRow dr = dt.NewRow();
                        for (int i = 0; i < headers.Length; i++)
                        {
                            dr[i] = rows[i].Trim();
                        }
                        dt.Rows.Add(dr);
                    }
                }

            }


            return dt;
        }

        public static DataTable ConvertXSLXtoDataTable(string strFilePath, string connString)
        {
            OleDbConnection oledbConn = new OleDbConnection(connString);
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            try
            {

                oledbConn.Open();
                using (DataTable Sheets = oledbConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null))
                {

                    for (int i = 0; i < Sheets.Rows.Count; i++)
                    {
                        string worksheets = Sheets.Rows[i]["TABLE_NAME"].ToString();
                        OleDbCommand cmd = new OleDbCommand(String.Format("SELECT * FROM [{0}]", worksheets), oledbConn);
                        OleDbDataAdapter oleda = new OleDbDataAdapter();
                        oleda.SelectCommand = cmd;
                        
                        oleda.Fill(ds);
                    }

                    dt = ds.Tables[0];
                    foreach (DataColumn dt_item in dt.Columns.OfType<DataColumn>())
                    {
                        dt_item.ColumnName = dt_item.ColumnName.Trim();
                    }
                }

            }
            catch (Exception ex)
            {
            }
            finally
            {

                oledbConn.Close();
            }

            return dt;

        }
        public static void PeriodFromStr(string str, out DateTime date_from, out DateTime date_to)
        {
            DateTime _date_from = DateTime.Now.ToString("MM/dd/yyyy").ToDateTime(format: "MM/dd/yyyy");
            DateTime _date_to = DateTime.Now.ToString("MM/dd/yyyy").ToDateTime(format: "MM/dd/yyyy");
            if (str.Contains("from"))
            {
                _date_to = ("01/01/2200").ToDateTime(format: "MM/dd/yyyy");
                str = str.Replace("from", "").Replace(" ", "");
                if(str.Length<6)
                {
                    str = str + "/" + DateTime.Now.Year.ToString();
                    str = BusinessLogic.RebuildDateStr(str);
                }
                _date_from= str.ToDateTime(format: "MM/dd/yyyy");
            }
            else if(str.Contains("-"))
            {
                string[] str_array = str.Split('-');
                if(str_array.Length>1)
                {
                    str_array[0] = str_array[0].Replace(" ", "");
                    str_array[1] = str_array[1].Replace(" ", "");
                    if (str_array[0].Length < 6)
                    {
                        
                        str_array[0] = str_array[0] + "/" + DateTime.Now.Year.ToString();
                        
                    }
                    if (str_array[1].Length < 6)
                    {
                        str_array[1] = str_array[1] + "/" + DateTime.Now.Year.ToString();
                    }
                    str_array[0] = BusinessLogic.RebuildDateStr(str_array[0]);
                    str_array[1] = BusinessLogic.RebuildDateStr(str_array[1]);

                    _date_from = str_array[0].ToDateTime(format: "MM/dd/yyyy");
                    _date_to = str_array[1].ToDateTime(format: "MM/dd/yyyy");
                }
                
            }
            date_from = _date_from;
            date_to = _date_to;
        }
        public static string RebuildDateStr(string str)
        {
            string result = str;
            string[] result_array = str.Split('/');
            for (int i = 0; i < result_array.Length; i++)
            {
                if (i < 2)
                {
                    result_array[i] = (result_array[i].Length < 2) ? ("0" + result_array[i]) : result_array[i];
                }
                else
                {
                    result_array[i] = (result_array[i].Length < 4) ? ("20" + result_array[i]) : result_array[i];
                }
            }
            result = String.Join("/", result_array);
            return result;
        }
        public static string DiscontsWrite(DataTable table)
        {
            string result="";
            try
            {
                string description = "";
                DateTime date_from = DateTime.Now;
                DateTime date_to = DateTime.Now;
                
                using (EntityframeWork.AdventureWorks2012_DataEntities entity = new EntityframeWork.AdventureWorks2012_DataEntities())
                {
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        if (String.IsNullOrEmpty(table.Rows[i]["DISCOUNT"] == null ? "" : table.Rows[i]["DISCOUNT"].ToString()))
                        {
                            description = table.Rows[i]["PRODUCT"] == null ? "" : table.Rows[i]["PRODUCT"].ToString();
                            string period = table.Rows[i]["PERIOD"] == null ? "" : table.Rows[i]["PERIOD"].ToString();
                            BusinessLogic.PeriodFromStr(period, out date_from, out date_to);
                        }
                        else
                        {
                            string product_name= (table.Rows[i]["PRODUCT"] == null ? "" : table.Rows[i]["PRODUCT"].ToString());
                            EntityframeWork.Product product = entity.Product.OfType<EntityframeWork.Product>().Where(t => t.Name == product_name).FirstOrDefault();
                            if (product != null)
                            {
                                List<EntityframeWork.Product_Disconts> product_disconts_list = entity.Product_Disconts.Count() == 0 ? null:
                                    (entity.Product_Disconts.OfType<EntityframeWork.Product_Disconts>()
                                    .Where(t => t.ProductID == product.ProductID && t.DateStart.ToString("yyyy-MM-dd") == date_from.ToString("yyyy-MM-dd") && t.DateFinish.ToString("yyyy-MM-dd") == date_to.ToString("yyyy-MM-dd")).ToList());
                                if (product_disconts_list!=null && product_disconts_list.Count > 0)
                                {
                                    foreach(EntityframeWork.Product_Disconts disc_item in product_disconts_list)
                                    {
                                        disc_item.Description = description;
                                        disc_item.Volume = float.Parse(table.Rows[i]["DISCOUNT"] == null ? "0" : table.Rows[i]["DISCOUNT"].ToString());
                                        disc_item.MinQuantity = String.IsNullOrEmpty(table.Rows[i]["MIN QTY"] == null ? "" : table.Rows[i]["MIN QTY"].ToString()) ? 1 : int.Parse(table.Rows[i]["MIN QTY"].ToString());
                                    }
                                }
                                else
                                {
                                    entity.Product_Disconts.Add(new EntityframeWork.Product_Disconts
                                    {

                                        DateFinish = date_to,
                                        DateStart = date_from,
                                        ProductID = product.ProductID,
                                        Volume = float.Parse(table.Rows[i]["DISCOUNT"] == null ? "0" : table.Rows[i]["DISCOUNT"].ToString()),
                                        Description = description,
                                        MinQuantity = String.IsNullOrEmpty(table.Rows[i]["MIN QTY"] == null ? "" : table.Rows[i]["MIN QTY"].ToString()) ? 1 : int.Parse(table.Rows[i]["MIN QTY"].ToString())
                                    });
                                    
                                }
                                
                            }
                        }
                    }
                    entity.SaveChanges();
                }

            }
            catch(Exception ex)
            {
                result += ex.Message;
            }
            return result;
        }
        
    }
}