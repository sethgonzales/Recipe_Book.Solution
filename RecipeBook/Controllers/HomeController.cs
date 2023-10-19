using Microsoft.AspNetCore.Mvc;
using RecipeBook.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace RecipeBook.Controllers
{
  public class HomeController : Controller
  {
    private readonly RecipeBookContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(UserManager<ApplicationUser> userManager, RecipeBookContext db)
    {
      _userManager = userManager;
      _db = db;
    }

    [HttpGet("/")]
    public ActionResult Index()
    {
      List<Recipe> recipes = _db.Recipes
                  .Include(recipe => recipe.User)
                  .ToList();

      return View(recipes);
    }
  }
}