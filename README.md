# Adding ASP.NET Auth to Blazor Server

# 1. NuGet Packages

### Required Auth Packages

```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="7.0.2" />
<PackageReference Include="Microsoft.AspNetCore.Identity.UI" Version="7.0.2" />
```

### To Run Migrations

You need the EF Core tools package and whatever EF Core provider is desired:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="7.0.2" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="7.0.2" />
```

---

# 2. Add Db

### Add a class that inherits from the default identity DbContext

```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bylines.Reauth.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}
```

### Ensure a connection string is present in appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "DataSource=app.db;Cache=Shared"
  },
[...]
}
```

### Then add the DbContext to the service collection:

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
```

### Add Migrations using the EF CLI

```bash
dotnet ef migrations add init
dotnet ef database update
```

---

# 3.  Add [ASP.NET](http://ASP.NET) Identity Services

### Add services so that Blazor can use them

```csharp
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
								.AddEntityFrameworkStores<ApplicationDbContext>();
```

### Create _LoginPartial.cshtml in /Areas/Identity/Pages/Shared

```csharp
@using Microsoft.AspNetCore.Identity
@inject SignInManager<IdentityUser> SignInManager
@inject UserManager<IdentityUser> UserManager
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers

<ul class="navbar-nav">
@if (SignInManager.IsSignedIn(User))
{
    <li class="nav-item">
        <a  class="nav-link text-dark" asp-area="Identity" asp-page="/Account/Manage/Index" title="Manage">Hello @User.Identity?.Name!</a>
    </li>
    <li class="nav-item">
        <form class="form-inline" asp-area="Identity" asp-page="/Account/Logout" asp-route-returnUrl="/" method="post">
            <button  type="submit" class="nav-link btn btn-link text-dark">Logout</button>
        </form>
    </li>
}
else
{
    <li class="nav-item">
        <a class="nav-link text-dark" asp-area="Identity" asp-page="/Account/Register">Register</a>
    </li>
    <li class="nav-item">
        <a class="nav-link text-dark" asp-area="Identity" asp-page="/Account/Login">Login</a>
    </li>
}
</ul>
```

### Add redirect component to handle unauthorized users

```html
@inject NavigationManager NavManager
@code 
{
    protected override void OnInitialized() => NavManager.NavigateTo("Identity/Account/Login", true);
}
```

### Modify App.razor to ensure redirect when not authorized

```html
<CascadingAuthenticationState>
    <AuthorizeView>
        <Authorized>
            <Router AppAssembly="@typeof(App).Assembly">
                <Found Context="routeData">
                    <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)"/>
                    <FocusOnNavigate RouteData="@routeData" Selector="h1"/>
                </Found>
                <NotFound>
                    <PageTitle>Not found</PageTitle>
                    <LayoutView Layout="@typeof(MainLayout)">
                        <p role="alert">Sorry, there's nothing at this address.</p>
                    </LayoutView>
                </NotFound>
            </Router>
        </Authorized>
        <NotAuthorized>
            <RedirectToLogin />
        </NotAuthorized>
    </AuthorizeView>
</CascadingAuthenticationState>
```