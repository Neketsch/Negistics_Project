using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.OleDb;
using System.IO;
using Negistics_Project.Common;
using ExtensionMethods;

namespace Negistics_Project.Controllers
{

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Disconts_ReceiverView()
        {
            ViewBag.Message = "Descont receiver page.";

            return View();
        }
       
        [ActionName("Disconts_receiverview")]
        [HttpPost]
        public ActionResult Disconts_ReceiverView1()
        {


            if (Request.Files["FileUpload1"].ContentLength > 0)
            {
                string extension = System.IO.Path.GetExtension(Request.Files["FileUpload1"].FileName).ToLower();
                string query = null;
                string connString = "";




                string[] validFileTypes = { ".xls", ".xlsx", ".csv" };

                string path1 = string.Format("{0}/{1}", Server.MapPath("~/Content/Uploads"), Request.Files["FileUpload1"].FileName);
                if (!Directory.Exists(path1))
                {
                    Directory.CreateDirectory(Server.MapPath("~/Content/Uploads"));
                }
                if (validFileTypes.Contains(extension))
                {
                    if (System.IO.File.Exists(path1))
                    {
                        System.IO.File.Delete(path1);
                    }
                    Request.Files["FileUpload1"].SaveAs(path1);
                    DataTable dt = null;
                    if (extension == ".csv")
                    {
                        dt = BusinessLogic.ConvertCSVtoDataTable(path1);
                       // ViewBag.Data = dt;
                    }
                    //Connection String to Excel Workbook  
                    else if (extension.Trim() == ".xls")
                    {
                        connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path1 + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                        dt = BusinessLogic.ConvertXSLXtoDataTable(path1, connString);
                       // ViewBag.Data = dt;
                    }
                    else if (extension.Trim() == ".xlsx")
                    {
                        connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path1 + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                        dt = BusinessLogic.ConvertXSLXtoDataTable(path1, connString);
                        //ViewBag.Data = dt;
                    }
                    if (dt != null)
                    {
                        ViewBag.Error = BusinessLogic.DiscontsWrite(dt);
                        using (EntityframeWork.AdventureWorks2012_DataEntities entity = new EntityframeWork.AdventureWorks2012_DataEntities())
                        {
                            ViewBag.Data = DateTableCustomExtensions.ToDataTable(entity.Product_Disconts.ToList());
                        }
                    }

                }
                else
                {
                    ViewBag.Error = "Please Upload Files in .xls, .xlsx or .csv format";

                }

            }

            return View();

        }
       
    }
}