using AutoMapper;
using AutoMapper.QueryableExtensions;
using EFCore7MappingIssue.Domain;
using EFCore7MappingIssue.Infrastructure;
using EFCore7MappingIssue.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("AppConnection")));
//builder.Services.AddDbContext<AppDbContext>(x => x.UseInMemoryDatabase(Guid.NewGuid().ToString()));

builder.Services.AddCustomAutoMapper(typeof(AppDbContext).Assembly);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await dbContext.Database.EnsureDeletedAsync();
    await dbContext.Database.EnsureCreatedAsync();

    dbContext.Camapaigns.AddRange(GetCampaigns());

    await dbContext.SaveChangesAsync();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

    var query = dbContext.Camapaigns.ProjectTo<CampaignSearchResultModel>(mapper.ConfigurationProvider);

    var result = await query.ToListAsync();
}

app.Run();


static List<Campaign> GetCampaigns()
{
    return new List<Campaign>
    {
        new Campaign { TriggerType = TriggerType.AccountSignedUp},
        new Campaign { TriggerType = null},
        new Campaign { TriggerType = TriggerType.ServiceHistoryLinkedToVehicle}
    };
}
