using System.Windows;
using PCBDetection.Detection;
using PCBDetection.Services;
using PCBDetection.Services.Interfaces;
using PCBDetection.Views;
using Prism.DryIoc;
using Prism.Ioc;

namespace PCBDetection;

public partial class App : PrismApplication
{
    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<ILogService, LogService>();
        containerRegistry.RegisterSingleton<ICameraService, MockCameraService>();
        containerRegistry.RegisterSingleton<IInspectionService, MockInspectionService>();
        containerRegistry.RegisterSingleton<IAiDetectionService, MockAiDetectionService>();
        containerRegistry.RegisterSingleton<IRecipeService, RecipeService>();
        containerRegistry.RegisterSingleton<ILightService, MockLightService>();
        containerRegistry.RegisterSingleton<IPlcService, MockPlcService>();
        containerRegistry.RegisterSingleton<IMesService, MockMesService>();
        containerRegistry.RegisterSingleton<IProductionStatisticsService, ProductionStatisticsService>();
        containerRegistry.RegisterSingleton<IInspectionWorkflowService, InspectionWorkflowService>();
    }
}
