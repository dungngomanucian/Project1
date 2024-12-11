using Project1.Models;
namespace Project1.Areas.Admin.ViewModels
{
    public class Email_Password_forAdmin
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public List<TUser> listAdmin {  get; set; }
    }
}
