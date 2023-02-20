using System.Text.Json;
using dotenv.net;
using Microsoft.AspNetCore.Http.Extensions;

// .envファイルの読み込み
DotEnv.Load(options: new DotEnvOptions(trimValues: true));
var envVars = DotEnv.Read();
var scheme = envVars["SCHEME"];
var host = envVars["HOST"];
var port = envVars["PORT"];

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.MapGet("/{num:int}", (int num, HttpContext context) =>
{
  // リクエストURLを取得
  var requestUrl = context.Request.GetDisplayUrl();
  Console.WriteLine($"Request Url -> {requestUrl}");

  if (num < 1)
  {
    return Results.BadRequest(new
    {
      error = "Number must be greater than 0.",
    });
  }

  if (num == 1)
  {
    return Results.Ok(new
    {
      num = 1,
    });
  }

  // HTTPリクエストを送信する
  var client = new HttpClient();
  var response = client.GetAsync($"{scheme}://{host}:{port}/{num - 1}").Result;
  var content = response.Content.ReadAsStringAsync().Result;
  var jsonDoc = JsonDocument.Parse(content);
  var n = jsonDoc.RootElement.GetProperty("num").GetInt32();

  return Results.Ok(new {
    num = num * n,
  });

})
.WithName("Factorial")
.WithOpenApi();

app.Run($"http://+:{port}");
