using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using RecipeBook.Models;
using System.Threading.Tasks;


// namespace Identity.Controllers
namespace RecipeBook.Controllers

{
  public class RoleController : Controller
  {
    private RoleManager<IdentityRole> roleManager;
    public RoleController(RoleManager<IdentityRole> roleMgr)
    {
      roleManager = roleMgr;
    }

    public ViewResult Index() => View(roleManager.Roles);

    private void Errors(IdentityResult result)
    {
      foreach (IdentityError error in result.Errors)
        ModelState.AddModelError("", error.Description);
    }
    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create([Required] string name) //use this to create a new identity role
    {
      if (ModelState.IsValid)
      {
        IdentityResult result = await roleManager.CreateAsync(new IdentityRole(name));
        if (result.Succeeded)
          return RedirectToAction("Index");
        else
          Errors(result);
      }
      return View(name);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
      IdentityRole role = await roleManager.FindByIdAsync(id);
      if (role != null)
      {
        IdentityResult result = await roleManager.DeleteAsync(role);
        if (result.Succeeded)
          return RedirectToAction("Index");
        else
          Errors(result);
      }
      else
        ModelState.AddModelError("", "No role found");
      return View("Index", roleManager.Roles);
    }
  }
}