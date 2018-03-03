using AutoMapper;
using TechnicalSupportService.Areas;

namespace TechnicalSupportService.App_Start
{
    public class AutomapperConfig
    {
        public static void Register()
        {
            ConfigureMapping();
        }
        private static void ConfigureMapping()
        {
            Mapper.Initialize(cfg => cfg.AddProfile<AutoMapperProfile>());
        }
    }
}