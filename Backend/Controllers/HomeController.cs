using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ClinicManagementSystem.Models;

namespace ClinicManagementSystem.Controllers;

public class HomeController : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public new IActionResult NotFound()
    {
        Response.StatusCode = 404;
        return View();
    }
}
