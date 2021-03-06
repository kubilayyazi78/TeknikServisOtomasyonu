﻿using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TeknikServis.BLL.Repository;
using TeknikServis.BLL.Services.Senders;
using TeknikServis.Models.Entities;
using TeknikServis.Models.Enums;
using TeknikServis.Models.Idendity_Models;
using TeknikServis.Models.ViewModels;
using static TeknikServis.BLL.Identity.MemberShipTools;

namespace TeknikServis.Web.UI.Controllers
{
    public class OperatorController : Controller
    {
        List<SelectListItem> ButunTeknisyenler = new List<SelectListItem>();
        // GET: Operator
        [Authorize(Roles = "Admin,Operator")]
        public ActionResult Index()
        {
            return View(new ArizaRepo().GetAll());
        }
        [Authorize(Roles = "Admin,Operator")]
        public ActionResult ArizaDetaySayfasi(int id = 0)
        {
            var data = new ArizaRepo().GetById(id);
            if (data == null)
                RedirectToAction("Index");

            var id2 = HttpContext.GetOwinContext().Authentication.User.Identity.GetUserId();
            var user = NewUserManager().FindById(id2);

            var TeknisyenRolu = NewRoleManager().FindByName("Teknisyen").Users.Select(x => x.UserId).ToList();


            var arizalar = new ArizaRepo().GetAll().ToList();

            var userManager = NewUserManager();
            var userlar = userManager.Users.ToList();
          //  var teknisyenId = new ArizaRepo().GetAll(x => x.ArizaTeknisyeneAtandiMi == true).Select(x => x.TeknisyenId).ToList();
            ButunTeknisyenler = new List<SelectListItem>();
            foreach (var item in userlar)
            {
                if (item.AtandiMi) continue;
                if (userManager.IsInRole(item.Id, IdentityRoles.Teknisyen.ToString()))
                {
                    ButunTeknisyenler.Add(new SelectListItem()
                    {

                        Text = item.Ad + " " + item.Soyad,
                        Value = item.Id
                    });
                }
            }



            ViewBag.TeknisyenK = ButunTeknisyenler;

            var model = new ArizaViewModel()
            {

                UrunTipi = data.UrunTipi,
                MusteriId = data.MusteriId,
                UrunAdi = data.UrunAdi,
                MusteriYorumu = data.MusteriYorumu,
                UrunResmi = data.UrunResmi,
                FaturaResmi = data.FaturaResmi,
                SehirAdi = data.SehirAdi,
                Adres = data.Adres,
                GarantiDurumu = data.GarantiDurumu,
                ArizaId = data.Id

            };

            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Operator")]
        public ActionResult OperatorAtama(ArizaViewModel model)
        {

            try
            {
                var id2 = HttpContext.GetOwinContext().Authentication.User.Identity.GetUserId();
                var user = NewUserManager().FindById(id2);
                var ariza = new ArizaRepo().GetById(model.ArizaId);
                if (user != null)
                {

                    ariza.OperatorId = user.Id;
                    ariza.ArizaKabulEdildiMi = true;
                    ariza.ArizaKabulTarihi = DateTime.Now;
                    new ArizaRepo().Update(ariza);


                }

            }
            catch (Exception)
            {


                TempData["Model"] = new ErrorViewModel()
                {
                    Text = $"Operator Atanması Sırasında Bir Hata Oluştu",
                    ActionName = "Index",
                    ControllerName = "Operator",
                    ErrorCode = 500
                };
                return RedirectToAction("Error", "Home");

            }
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> TeknisyenAtama(ArizaViewModel model)

        {

            var userStore = NewUserStore();

            var TeknisyenRolu = NewRoleManager().FindByName("Teknisyen").Users.Select(x => x.UserId).ToList();



            for (int i = 0; i < TeknisyenRolu.Count; i++)
            {

                var User = await userStore.FindByIdAsync(TeknisyenRolu[i]);
                if (User.Id == model.TeknisyenId)
                {
                    User.AtandiMi = true;
                    await userStore.UpdateAsync(User);
                    userStore.Context.SaveChanges();
                    break;

                }

            }
            try
            {
                var teknisyen = NewUserManager().FindById(model.TeknisyenId);
                var ariza = new ArizaRepo().GetById(model.ArizaId);
                if (teknisyen != null)
                {

                    ariza.TeknisyenId = teknisyen.Id;
                    ariza.ArizaTeknisyeneAtandiMi = true;

                    new ArizaRepo().Update(ariza);

                }
                else
                {
                    throw new Exception("Teknisyen atama işlemi sırasında bir hata oluştu !");
                }
                var emailService = new EmailService();
                var body = $"Merhaba <b>{teknisyen.Ad} {teknisyen.Soyad}</b><br> <b>Tipi:</b>{ariza.UrunTipi} <b>Modeli:</b>{ariza.UrunAdi}olan arızalı ürün tamir edilmek üzere şahsınıza atanmıştır.";
                await emailService.SendAsync(new IdentityMessage() { Body = body, Subject = "Arıza Ataması" }, teknisyen.Email);

            }
            catch (Exception)
            {


                TempData["Model"] = new ErrorViewModel()
                {
                    Text = $"Teknisyen Ataması Sırasında Bir Hata oluştu !",
                    ActionName = "Index",
                    ControllerName = "Operator",
                    ErrorCode = 500
                };
                return RedirectToAction("Error", "Home");

            }

            return RedirectToAction("Index");

        }
    }
}