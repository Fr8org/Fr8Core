using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.Controllers
{
    public class ProcessTemplateController : Controller
    {
        // GET: ProcessTemplate
        public ActionResult Index()
        {
            return View();
        }

        // GET: ProcessTemplate/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ProcessTemplate/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ProcessTemplate/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: ProcessTemplate/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ProcessTemplate/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: ProcessTemplate/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ProcessTemplate/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
