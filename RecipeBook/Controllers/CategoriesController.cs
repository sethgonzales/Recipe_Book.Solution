using Microsoft.AspNetCore.Mvc;
using RecipeBook.Models;
using System.Collections.Generic;
using System.Linq;

namespace RecipeBook.Controllers
{
  public class CategoriesController : Controller
  {
    private readonly RecipeBookContext _db;

    public CategoriesController(RecipeBookContext db)
    {
      _db = db;
    }

    public ActionResult Index()
    {
      List<Category> model = _db.Categories.ToList();
      return View(model);
    }
  }
}