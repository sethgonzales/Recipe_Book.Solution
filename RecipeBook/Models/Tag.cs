using System.Collections.Generic;

namespace RecipeBook.Models
{
  public class Tag
    {
        public int TagId { get; set; }
        public string Title { get; set; }
        public List<RecipeTag> JoinEntities { get;}
    }
}