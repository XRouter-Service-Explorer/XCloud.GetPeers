using Blocknet.Lib.CoinConfig;
using Blocknet.Lib.Services;
using Blocknet.Lib.Services.Coins.Base;
using Blocknet.Lib.Services.Coins.Cryptocoin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XCloud.GetPeers.Api.Persistance;
using XCloud.GetPeers.Api.Services;

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

            var rpcSettings = Configuration.GetSection("CoinConfig").Get<CoinRpcConfig>();
            services.AddTransient<ICoinService, CoinService>();

            services.AddTransient<ICryptocoinService>(service =>
            {
                var svc = new CryptocoinService(
                    //rpcSettings.Blocknet.DaemonUrl_testnet, 
                    rpcSettings.CryptoCoin.DaemonUrl,
                    rpcSettings.CryptoCoin.RpcUserName,
                    rpcSettings.CryptoCoin.RpcPassword,
                    rpcSettings.CryptoCoin.WalletPassword,
                    rpcSettings.CryptoCoin.RpcRequestTimeoutInSeconds
                    );
                svc.Parameters.CoinShortName = rpcSettings.CryptoCoin.CoinShortName;
                svc.Parameters.CoinLongName = rpcSettings.CryptoCoin.CoinLongName;
                return svc;
            });

            services.AddSingleton<IPeerListRepository, PeerListRepository>();
            services.AddHostedService<PeerListService>();
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
