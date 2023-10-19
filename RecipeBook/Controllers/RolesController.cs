using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using RecipeBook.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

// namespace Identity.Controllers
namespace RecipeBook.Controllers
{
  [Authorize(Roles = "Admin")]
  public class RoleController : Controller
  {
    private RoleManager<IdentityRole> roleManager;
    private UserManager<ApplicationUser> userManager;

    public RoleController(RoleManager<IdentityRole> roleMgr, UserManager<ApplicationUser> userMrg)
    {
      roleManager = roleMgr;
      userManager = userMrg;
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
    public async Task<IActionResult> Update(string id)
    {
      IdentityRole role = await roleManager.FindByIdAsync(id);
      List<ApplicationUser> members = new List<ApplicationUser>();
      List<ApplicationUser> nonMembers = new List<ApplicationUser>();
      List<ApplicationUser> users = await userManager.Users.ToListAsync();

      foreach (ApplicationUser user in users)
      {
        var list = await userManager.IsInRoleAsync(user, role.Name) ? members : nonMembers;
        list.Add(user);
      }
      return View(new RoleEdit
      {
        Role = role,
        Members = members,
        NonMembers = nonMembers
      });
    }

    [HttpPost]
    public async Task<IActionResult> Update(RoleModification model)
    {
      IdentityResult result;
      if (ModelState.IsValid)
      {
        foreach (string userId in model.AddIds ?? new string[] { })
        {
          ApplicationUser user = await userManager.FindByIdAsync(userId);
          if (user != null)
          {
            result = await userManager.AddToRoleAsync(user, model.RoleName);
            if (!result.Succeeded)
              Errors(result);
          }
        }
        foreach (string userId in model.DeleteIds ?? new string[] { })
        {
          ApplicationUser user = await userManager.FindByIdAsync(userId);
          if (user != null)
          {
            result = await userManager.RemoveFromRoleAsync(user, model.RoleName);
            if (!result.Succeeded)
              Errors(result);
          }
        }
      }

      if (ModelState.IsValid)
        return RedirectToAction(nameof(Index));
      else
        return await Update(model.RoleId);
    }
  }
}