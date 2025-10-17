using HardwareStore.Services.Interfaces;
using HardwareStore.WebClient.ViewModels.Forms;
using Microsoft.AspNetCore.Mvc;

namespace HardwareStore.WebClient.Controllers
{
    public class FooterController : Controller
    {
        private readonly IEmailService _emailService;

        public FooterController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult ContactUs() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ContactUs(ContactUsViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            string body = $@"
               <h3>New Contact Message</h3>
                <p><strong>Name:</strong> {viewModel.Name}</p>
                <p><strong>Email:</strong> {viewModel.Email}</p>
                <p><strong>Subject:</strong> {viewModel.Subject}</p>
                <p><strong>Message:</strong><br>{viewModel.Message}</p>";

            _emailService.SendEmail("authtesting@gmail.com", $"Contact Us: {viewModel.Subject}", body);

            TempData["SuccessMessage"] = "Your message has been sent successfully. We will get back to you shortly.";
            return RedirectToAction("ContactUs");
        }
    }
}