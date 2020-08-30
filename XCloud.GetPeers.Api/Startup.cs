using Blocknet.Lib.CoinConfig;
using Blocknet.Lib.Services;
using Blocknet.Lib.Services.Coins.Base;
using Blocknet.Lib.Services.Coins.Cryptocoin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using XCloud.GetPeers.Api.Persistance;

namespace XCloud.GetPeers.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            var rpcInputs = Configuration.GetSection("CoinConfig").Get<List<RpcInput>>();
            services.AddTransient<ICoinService, CoinService>();

            foreach (var rpcInput in rpcInputs)
            {
                services.AddTransient<ICryptocoinService>(service =>
                {
                    var svc = new CryptocoinService(
                        rpcInput.DaemonUrl,
                        rpcInput.RpcUserName,
                        rpcInput.RpcPassword,
                        rpcInput.WalletPassword,
                        rpcInput.RpcRequestTimeoutInSeconds
                        );
                    svc.Parameters.CoinShortName = rpcInput.CoinShortName;
                    svc.Parameters.CoinLongName = rpcInput.CoinLongName;
                    return svc;
                });
            }

            services.AddSingleton<IPeerListRepository, PeerListRepository>();
            //services.AddHostedService<PeerListService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
