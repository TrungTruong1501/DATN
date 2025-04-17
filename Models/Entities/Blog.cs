using System.ComponentModel.DataAnnotations;

namespace FashionShop.Models.Entities
{
    public class Blog
    {       
        [Key]
        public int id { get; set; }
        public string title { get; set; }
        public string contentBlog { get; set; }
        public string thumbnail { get; set; }
        public string date { get; set; }
        public string author { get; set; }
        public string RowState { get; set; } = "Unchanged";
    }
}