using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Xunit;
using RaceDay.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDay.Api.Data; 

namespace RaceDay.Api.Tests.Infrastructure
{
    public class RaceDayApiFactory
    : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(
            IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<
                    DbContextOptions<RaceDayDbContext>>();

                services.AddDbContext<RaceDayDbContext>(
                    options => options.UseSqlServer(
                        "Server=LabVM2049939\\SQLEXPRESS;" +
                        "Database=RaceDayTests;" +
                        "Trusted_Connection=True;" +
                        "TrustServerCertificate=True;"));
            });
        }
    }
}
