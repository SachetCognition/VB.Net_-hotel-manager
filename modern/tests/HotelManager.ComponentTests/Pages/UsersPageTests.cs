using HotelManager.Web.Components.Pages;
using MudBlazor;

namespace HotelManager.ComponentTests.Pages;

public class UsersPageTests : ComponentTestBase
{
    [Fact]
    public void Renders_existing_users()
    {
        var cut = RenderPage<Users>();
        Assert.Contains("admin", cut.Markup);
        AuthSvc.Verify(s => s.GetUsersAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public void AddUser_with_empty_fields_warns_and_does_not_create()
    {
        var cut = RenderPage<Users>();
        ClickButton(cut, "Add");
        AuthSvc.Verify(s => s.CreateUserAsync(It.IsAny<Registration>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void AddUser_with_values_creates_user()
    {
        var cut = RenderPage<Users>();
        cut.FindAll("input")[0].Change("newuser");
        cut.Find("input[type=password]").Change("secret");
        ClickButton(cut, "Add");
        AuthSvc.Verify(s => s.CreateUserAsync(It.Is<Registration>(r => r.UserName == "newuser"), "secret"), Times.Once);
    }

    [Fact]
    public void AddUser_existing_username_reports_error()
    {
        AuthSvc.Setup(s => s.GetUserAsync("dup")).ReturnsAsync(new Registration { UserName = "dup" });
        var cut = RenderPage<Users>();
        cut.FindAll("input")[0].Change("dup");
        cut.Find("input[type=password]").Change("secret");
        ClickButton(cut, "Add");
        AuthSvc.Verify(s => s.CreateUserAsync(It.IsAny<Registration>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void DeleteUser_removes_user()
    {
        var cut = RenderPage<Users>();
        ClickIcon(cut, Icons.Material.Filled.Delete);
        AuthSvc.Verify(s => s.DeleteUserAsync("admin"), Times.Once);
    }
}
