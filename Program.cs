using FashionShop.Models;
using FashionShop.Models.Entities;
using FashionShop.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

var builder = WebApplication.CreateBuilder(args);

//------------------------------------------------------
// Đăng ký các dịch vụ
//------------------------------------------------------

// Đăng ký MVC
builder.Services.AddControllersWithViews();

// Cấu hình cơ sở dữ liệu
builder.Services.AddDbContext<FashionShopContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình Session và bộ nhớ đệm
builder.Services.AddDistributedMemoryCache(); // Lưu trữ dữ liệu session
builder.Services.AddMemoryCache(); // Bộ nhớ đệm cho ChatbotService

// Cấu hình Session với các tùy chọn cụ thể
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "FashionShop.Session"; // Tên cookie rõ ràng
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn session
    options.Cookie.HttpOnly = true; // Bảo mật cookie
    options.Cookie.IsEssential = true; // Đánh dấu cookie này là cần thiết
});

// Thêm HTTP Context Accessor để truy cập Session trong dịch vụ
builder.Services.AddHttpContextAccessor();

// Đăng ký dịch vụ HTTP Client để gọi API OpenAI
builder.Services.AddHttpClient();

// Đăng ký các dịch vụ của ứng dụng
builder.Services.AddScoped<ChatbotService>(); // Dịch vụ chatbot
builder.Services.AddScoped<ProductRecommendationService>(); // Dịch vụ gợi ý sản phẩm

//------------------------------------------------------
// Xây dựng ứng dụng
//------------------------------------------------------

var app = builder.Build();

// Cấu hình HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Môi trường phát triển
    app.UseDeveloperExceptionPage(); // Hiển thị chi tiết lỗi
}
else
{
    // Môi trường production
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Bật HSTS
}

// Các middleware cơ bản
app.UseHttpsRedirection(); // Chuyển hướng HTTP sang HTTPS
app.UseStaticFiles(); // Phục vụ file tĩnh (CSS, JS, hình ảnh,...)

// Middleware định tuyến
app.UseRouting();

// Middleware xác thực và ủy quyền
app.UseAuthorization();

// Middleware session - QUAN TRỌNG: Đặt TRƯỚC middleware endpoint
app.UseSession();

// Định nghĩa các route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Chạy ứng dụng
app.Run();