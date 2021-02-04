using _2FaGoogleAuthenticator.ViewModel;
using Google.Authenticator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

/// <summary>
/// Ref: https://www.youtube.com/watch?v=04by8038L_I
/// </summary>
namespace _2FaGoogleAuthenticator.Controllers
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


        /* ============================  2FA Start ========================*/
        private const string key = "qaz123!@@()*"; // any 10-12 char string as private key for Google Authenticator
        public ActionResult Login()
        {
            return View(); 
        }

        [HttpPost]
        public ActionResult Login(LoginModel loginModel)
        {
            string message = "";
            bool status = false;

            // Check username and password from our db here 

            if (loginModel.UserName == "Admin" && loginModel.Password == "Password1")
            {
                status = true;
                message = "2FA Verification";
                Session["Username"] = loginModel.UserName;

                // 2FA setup 
                TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
                string UserUniqueKey = loginModel.UserName + key; // must be encrypted 

                Session["UserUniqueKey"] = UserUniqueKey;

                var setupInfo = tfa.GenerateSetupCode("IdentityStandaloneMfa", loginModel.UserName, UserUniqueKey, false);

                
                ViewBag.BarcodeImageUrl = setupInfo.QrCodeSetupImageUrl;
                ViewBag.Barcode = setupInfo.ManualEntryKey;
            }
            else
            {
                message = "Invalid Credential"; 
            }

            ViewBag.Message = message; 
            ViewBag.Status = status;

            return View();
            //return RedirectToAction("EnrollApp", "Home");
        }

        public ActionResult EnrollApp()
        {
            string message = "";
            bool status = false;

            status = true;
            message = "2FA Verification";

            var userName = Session["Username"].ToString(); 
            // 2FA setup 
            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
            string UserUniqueKey = userName + key; // must be encrypted 

            UserUniqueKey = Session["UserUniqueKey"].ToString();

            var setupInfo = tfa.GenerateSetupCode("IdentityStandaloneMfa", userName, UserUniqueKey, false, 300);


            ViewBag.BarcodeImageUrl = setupInfo.QrCodeSetupImageUrl;
            ViewBag.Barcode = setupInfo.ManualEntryKey;

            return View(); 
        }

        public ActionResult MyProfile()
        {
            if (Session["Username"] == null || Session["IsValid2FA"] == null || ! (bool)Session["IsValid2FA"])
            {
                return RedirectToAction("Login"); 
            }

            ViewBag.Message = "Welcome " + Session["Username"].ToString();

            return View(); 
        }

        [HttpPost]
        public ActionResult Verify2Fa(string token)
        {
            //var token = Request["password"];

            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
            string UserUniqueKey = Session["UserUniqueKey"].ToString();

            /// Checking with Google App code
            bool isValid = tfa.ValidateTwoFactorPIN(UserUniqueKey, token); 


            if (isValid)
            {
                Session["IsValid2FA"] = true;
                return RedirectToAction("MyProfile", "Home"); 
            }

            return RedirectToAction("Login", "Home");

        }



        /* ============================  2FA End  ========================*/




    }
}