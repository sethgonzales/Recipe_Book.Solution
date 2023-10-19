using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace RecipeBook.Controllers
{
  [Authorize]
  public class RecipesController : Controller
  {
    private readonly RecipeBookContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public RecipesController(UserManager<ApplicationUser> userManager, RecipeBookContext db)
    {
      _userManager = userManager;
      _db = db;
    }

    public async Task<ActionResult> Index()
    {
      string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      ApplicationUser currentUser = await _userManager.FindByIdAsync(userId);
      List<Recipe> userRecipes = _db.Recipes
                          .Where(entry => entry.User.Id == currentUser.Id)
                          .Include(recipe => recipe.Category)
                          .ToList();
      return View(userRecipes);
    }

    public ActionResult Create()
    {
      ViewBag.CategoryId = new SelectList(_db.Categories, "CategoryId", "Name");
      return View();
    }

    [HttpPost]
    public async Task<ActionResult> Create(Recipe recipe, int CategoryId)
    {
      if (!ModelState.IsValid)
      {
        ViewBag.CategoryId = new SelectList(_db.Categories, "CategoryId", "Name");
        return View(recipe);
      }
      else
      {
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        ApplicationUser currentUser = await _userManager.FindByIdAsync(userId);
        recipe.User = currentUser;
        _db.Recipes.Add(recipe);
        _db.SaveChanges();
        return RedirectToAction("Index");
      }
    }
    [AllowAnonymous]
    public ActionResult Details(int id)
    {
      Recipe thisRecipe = _db.Recipes.Include(recipe => recipe.Category).Include(recipe => recipe.JoinEntities).ThenInclude(join => join.Tag).FirstOrDefault(recipe => recipe.RecipeId == id);
      return View(thisRecipe);
    }

    // [Authorize(Roles = "Administrator, Editor")]
    public ActionResult Edit(int id)
    {
      Recipe thisRecipe = _db.Recipes.FirstOrDefault(recipe => recipe.RecipeId == id);
      ViewBag.CategoryId = new SelectList(_db.Categories, "CategoryId", "Name");
      if (User.IsInRole("Admin") || thisRecipe.User.Id == _userManager.GetUserId(User))
      {
        return View(thisRecipe);
      }
      else
      {
        return View("Unauthorized");
      }

    }

    [HttpPost]
    public ActionResult Edit(Recipe recipe)
    {
      if (User.IsInRole("Admin") || recipe.User.Id == _userManager.GetUserId(User))
      {
        _db.Recipes.Update(recipe);
        _db.SaveChanges();
        return RedirectToAction("Index");
      }
      else
      {
        return View("Unauthorized");
      }
    }


    public ActionResult Delete(int id)
    {
      Recipe thisRecipe = _db.Recipes.FirstOrDefault(recipe => recipe.RecipeId == id);
      return View(thisRecipe);
    }

    [HttpPost, ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id)
    {
      Recipe thisRecipe = _db.Recipes.FirstOrDefault(recipe => recipe.RecipeId == id);
      _db.Recipes.Remove(thisRecipe);
      _db.SaveChanges();
      return RedirectToAction("Index");
    }

    public ActionResult AddTag(int id)
    {
      Recipe thisRecipe = _db.Recipes.FirstOrDefault(recipe => recipe.RecipeId == id);
      ViewBag.TagId = new SelectList(_db.Tags, "TagId", "Title");
      return View(thisRecipe);
    }

    [HttpPost]
    public ActionResult AddTag(Recipe recipe, int TagId)
    {
      if (TagId != 0)
      {
        _db.RecipeTags.Add(new RecipeTag() { TagId = TagId, RecipeId = recipe.RecipeId });
        _db.SaveChanges();
      }
      return RedirectToAction("Details", new { id = recipe.RecipeId });
    }


    [HttpPost]
    public ActionResult DeleteJoin(int joinId)
    {
      RecipeTag joinEntry = _db.RecipeTags.FirstOrDefault(entry => entry.RecipeTagId == joinId);
      _db.RecipeTags.Remove(joinEntry);
      _db.SaveChanges();
      return RedirectToAction("Index");
    }

    public ActionResult Search(string searchRecipes)
    {
      List<Recipe> searchResults = _db.Recipes.Where(recipe => recipe.Name.Contains(searchRecipes) || recipe.Ingredients.Contains(searchRecipes)).ToList();
      return View("Search", searchResults);
    }
  }
}