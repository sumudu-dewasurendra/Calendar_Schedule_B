using Autofac;
using Autofac.Integration.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Owin;
using acpCommon.Context;
using acpMetaDataStorage.Repository;
using acpMetaDataLogic.Services;
using acpCommon.Mappers;
using acpCommon.Autofac;
using acpCommon.Repository;
using acpMetaDataStorage.Models;
using acpCommon.Services;
using acpDtoModel.Models;
using acpImportExport.Export;
using acpMetaDataStorage.Repository.Factory;
using acpAdminLogic.Services;
using acpMetaDataStorage.Repository.Factories.Datatypes;
using acpMetaDataLogic.Tokenizer;
using acpMetaDataLogic.Tokenizer.Readers;
using acpMetaDataLogic.Decoder;
using acpMetaDataStorage.Repository.Factories.QueryBuilders;
using acpMetaDataStorage.Repository.Audit;
using acpAdminStorage.Models;
using acpAdminStorage.Repository;
using acpMetaDataStorage.Repository.Factories.Connection;
using Autofac.Integration.SignalR;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using acpMetaDataWebApi.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Configuration;
using acpMetaDataLogic.Services.AOS;
using acpMetaDataLogic.Connectors;
using TuesPechkin;
using acpMetaDataLogic.Services.BarcodeFactory;

namespace acpMetaDataWebApi.Autofac
{
    /// <summary>
    /// Register the filters and components in autofac component register.
    /// </summary>
    public class AutofacRegister : IDependencyRegister
    {
        protected ContainerBuilder builder;
        private HttpConfiguration config;
        private IAppBuilder app;
        private HubConfiguration hubConfiguration;

        /// <summary>
        /// Constructs the Autofac Register.
        /// </summary>
        public AutofacRegister(IAppBuilder app, HttpConfiguration config)
        {
            this.app = app;
            this.config = config;

            builder = new ContainerBuilder();

            // SignalR Register hub
            hubConfiguration = new HubConfiguration();
            builder.RegisterHubs(Assembly.GetExecutingAssembly());
            builder.RegisterInstance(hubConfiguration);
        }

        /// <summary>
        /// Register Components and Filters to Autofac. 
        /// </summary>
        public void register()
        {
            //registerFilters();

            registerComponents();
        }

        private IContainer container;

        /// <summary>
        /// Publish the autofac Register.
        /// </summary>
        public void publish()
        {
            // Set the dependency resolver to be Autofac.
            container = builder.Build();
            config.DependencyResolver = new acpCommon.Autofac.AutofacWebApiDependencyResolver(container);

            // SignalR set the dependency resolver to be Autofac.
            hubConfiguration.Resolver = new AutofacDependencyResolver(container);

            app.UseAutofacMiddleware(container);
            app.UseAutofacWebApi(config);
            app.UseWebApi(config);

            app.MapSignalR("/signalr", hubConfiguration);           

            // Register add custom HubPipeline modules
            var hubPipeline = hubConfiguration.Resolver.Resolve<IHubPipeline>();
            hubPipeline.AddModule(new LoggingPipelineModule());
        }

        /// <summary>
        /// Register data persistence context.
        /// </summary>
        protected virtual void contextConfiguration()
        {
            builder.RegisterType<acpMetaDataStorage.Repository.AcpEntityContextFactory>().As<IDbContextFactory>();
            //builder.RegisterType<SqlDbConnection>().As<IDatabaseConnection>();
            builder.RegisterType<AcpDbConnectionFactory>().As<DbConnectionFactory>();
            builder.RegisterType<UserContextService>().As<IUserContextService>();    
        }

        /// <summary>
        /// Register Filters in autofac container.
        /// </summary>
        private void registerFilters()
        {
            //  Register the Autofac filter provider.
            builder.RegisterWebApiFilterProvider(config);
        }

        /// <summary>
        /// Register components in autofac container.
        /// </summary>
        private void registerComponents()
        {
            registerWebControlls();

            registerBusinessLogicComponents();
            MockInjection();

            contextConfiguration();

            registerRepositoryComponents();
            registerPublishAndAuditRepositories();
        }

        protected virtual void MockInjection()
        {
            builder.RegisterType<AcpConnectorServiceFactory>().As<ConnectorServiceFactory>();

            builder.RegisterType<SqlDbConnection>().As<IDatabaseConnection>();
        }

        /// <summary>
        /// Register all Web API controllers.
        /// </summary>  
        private void registerWebControlls()
        {
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            builder.RegisterAssemblyTypes(
           Assembly.GetExecutingAssembly())
               .Where(t =>
                !t.IsAbstract && typeof(ApiController).IsAssignableFrom(t))
               .InstancePerDependency();
        }

        /// <summary>
        /// Register the components in business logic.
        /// </summary>
        private void registerBusinessLogicComponents()
        { 
            // Admin Logic services
            builder.RegisterType<AcpConfigurationReaderService>().As<IAcpConfigurationReaderService>();
            builder.RegisterType<GdAccessFilterService>().As<IGdAccessFilterService>();   
            builder.RegisterType<TenantService>().As<ITenantService>();
            builder.RegisterType<MenuService>().As<IMenuService>();
            builder.RegisterType<GeneralDataAccessService>().As<IGeneralDataAccessService>();
            builder.RegisterType<EnvironmentModuleService>().As<IEnvironmentModuleService>();
            builder.RegisterType<AcpDataEntityTransformer>().As<DataEntityTransformer>();
            builder.RegisterType<GroupService>().As<IGroupService>();
            builder.RegisterType<GroupPermissionService>().As<IGroupPermissionService>();
            builder.RegisterType<UserPreferenceService>().As<IService<UserPreferenceDto>>();
            builder.RegisterType<ModuleService>().As<IModuleService>();
            builder.RegisterType<ModuleAccessService>().As<IModuleAccessService>();

            // MapperFactories and Mappers. 
            builder.RegisterType<AcpDataEntityTransformer>().As<DataEntityTransformer>();        

            // Services.
            builder.RegisterType<MetadataSchemaService>().As<IMetadataSchemaService>();
            builder.RegisterType<AcpDataTypeService>().As<IService<GdAcpDataTypeDto>>();
            builder.RegisterType<UIControlService>().As<IService<GdUIControlDto>>();
            builder.RegisterType<GdProcessingService>().As<IGdProcessingService>();
            builder.RegisterType<UserScheduleService>().As<IService<UserScheduleDto>>();
           // builder.RegisterType<AcpConnectorFactory>().As<ConnectorFactory>();
            builder.RegisterType<GdConnectionSettingService>().As<IGdConnectionSettingService>();

            builder.RegisterType<ModuleGdConfigurationService>().As<IModuleGdConfigurationService>();
            builder.RegisterType<PublishingService>().As<IPublishingService>();
            builder.RegisterType<AuditingService>().As<IAuditingService>();
            builder.RegisterType<DataComparator>().As<IDataComparator>();
            builder.RegisterType<ProjectService>().As<IService<GdProjectDto>>();
            builder.RegisterType<ProjectAssetService>().As<IProjectAssetService>();
            builder.RegisterType<DataFieldService>().As<IDataFieldService>();
            builder.RegisterType<TableConstraintService>().As<ITableConstraintService>();
            builder.RegisterType<GdExportService>().As<IGdExportService>();
            builder.RegisterType<ExportManager>().As<ExportManager>();
            builder.RegisterType<DataTypeConversion>().As<DataTypeConversion>();
            builder.RegisterType<OverruleUiControlSettingService>().As<IOverruleUiControlSettingService>();
            builder.RegisterType<HeaderTranslationService>().As<IHeaderTranslationService>();
            builder.RegisterType<PrimaryKeyConstraintService>().As<IService<GdPrimaryKeyConstraintDto>>();
            builder.RegisterType<ForeignKeyConstraintService>().As<IService<GdForeignKeyConstraintDto>>();
            builder.RegisterType<IndexConstraintService>().As<IService<GdIndexConstraintDto>>();

            builder.RegisterType<GdProcessingTranslationService>().As<IGdProcessingTranslationService>();          
            
            builder.RegisterType<DataFieldGroupService>().As<IDataFieldGroupService>();
            builder.RegisterType<ActionConfigurationService>().As<IActionConfigurationService>();
            builder.RegisterType<DbmsService>().As<IService<DbmsDto>>();
            builder.RegisterType<DbmsDataTypeService>().As<IService<DbmsDataTypeDto>>();
            builder.RegisterType<DbmsFunctionService>().As<IService<DbmsFunctionDto>>();    

            //builder.RegisterType<ODataToSqlDecoder>().As<IoDataDecoder>();
            builder.RegisterType<OdataQueryTokenizer>().As<IQueryTokenizer>();
            builder.RegisterType<OdataStringReader>().As<StringReader>();
            builder.RegisterType<OdataFunctionReaderFactory>().As<FunctionReaderFactory>();           
            builder.RegisterType<MetadataAuditService>().As<IMetadataAuditService>();
            builder.RegisterType<PublishingAuditService>().As<IPublishingAuditService>();
            builder.RegisterType<UserService>().As<IUserService>();
            builder.RegisterType<UserCredentialService>().As<IService<UserCredentialDto>>();
            builder.RegisterType<UserSlotWidgetService>().As<IUserSlotWidgetService>();

            builder.RegisterType<WidgetService>().As<IWidgetService>();
            builder.RegisterType<ChartWidgetConfigService>().As<IChartWidgetConfigService>();   

            builder.RegisterType<CssService>().As<ICssService>();  
            builder.RegisterType<XSDParserService>().As<IXSDParserService>();   
            builder.RegisterType<LabelPrintService>().As<ILabelPrintService>();

            builder.RegisterType<CustomPropertyFilterService>().As<IService<CustomPropertyFilterDto>>();
            builder.RegisterType<CustomPropertyDefinitionService>().As<IService<CustomPropertyDefinitionDto>>();
            builder.RegisterType<CustomPropertyValueService>().As<IService<CustomPropertyValueDto>>();
            builder.RegisterType<RemoteRestInvokeService>().As<IRemoteRestInvokeService>();
            builder.RegisterType<SchemaDynamicButtonService>().As<ISchemaDynamicButtonService>();
            builder.RegisterType<GdDynamicButtonService>().As<IGdDynamicButtonService>();

            // SIgnalR register hub context provider
            builder.RegisterType<HubContextProvider>().As<IHubContextProvider>();
            builder.RegisterType<MasterTableNameTranslationService>().As<IMasterTableNameTranslationService>();
            builder.RegisterType<AOSService>().As<IAOSService>();
            builder.RegisterType<AOSServiceXSLT>().As<IAOSServiceXSLT>();
            builder.RegisterType<FunctionScriptService>().As<IFunctionScriptService>();
            builder.RegisterType<LinkManagerStoreService>().As<ILinkManagerStoreService>();
            builder.RegisterType<MassUpdateService>().As<IMassUpdateService>();

            var pdfConverter = new ThreadSafeConverter(new RemotingToolset<PdfToolset>(new WinAnyCPUEmbeddedDeployment(new TempFolderDeployment())));
            builder.Register(c => pdfConverter).As<IConverter>().SingleInstance();

            builder.RegisterType<AcpBarcodeCreatorFactory>().As<AbstractBarcodeCreatorFactory>();
        }


        /// <summary>
        /// Register the components in data access logic.
        /// </summary>        
        private void registerRepositoryComponents()
        {
            // Admin Logic Repository           
            builder.Register((c, p) => new acpAdminStorage.Repository.AcpConfigurationReaderRepository(
               new acpAdminStorage.Repository.AcpEntityContextFactory()               
               )).As<IRepository<acpConfiguration>>();


            builder.RegisterType<acpAdminStorage.Repository.LanguageCodeRepository>().As<acpAdminStorage.Repository.ILanguageRepository>();
            builder.RegisterType<acpAdminStorage.Repository.UserPreferenceRepository>().As<IRepository<acpAdminStorage.Models.userPreference>>();            
            builder.RegisterType<GroupRepository>().As<IGroupRepository>();
            builder.RegisterType<GdViewRepository>().As<IGdViewRepository>();
            builder.RegisterType<GroupPermissionRepository>().As<IGroupPermissionRepository>();
            builder.RegisterType<UserPreferenceRepository>().As<IRepository<userPreference>>();
            builder.RegisterType<GroupFilterTemplateValueRepository>().As<IGroupFilterTemplateValueRepository>();
            builder.RegisterType<ModuleRepository>().As<IRepository<module>>();
            builder.RegisterType<ModuleAccessRepository>().As<IModuleAccessRepository>();
            builder.RegisterType<MenuActionRepository>().As<IRepository<menuAction>>();

            builder.Register((c, p) => new acpAdminStorage.Repository.UserRepository(
                new acpAdminStorage.Repository.AcpEntityContextFactory(),
                new acpAdminStorage.Repository.RootAdminRepository(new acpAdminStorage.Repository.AcpEntityContextFactory()),
                new acpAdminStorage.Repository.UserTranslationRepository(new acpAdminStorage.Repository.AcpEntityContextFactory()),
                new acpAdminStorage.Repository.GdViewRepository(new acpAdminStorage.Repository.AcpEntityContextFactory()),
                new acpAdminStorage.Repository.UserCredentialRepository(new acpAdminStorage.Repository.AcpEntityContextFactory()),
                new acpAdminStorage.Repository.UserLoginHistoryRepository(new acpAdminStorage.Repository.AcpEntityContextFactory()),
                new acpAdminStorage.Repository.UserPreferenceRepository(new acpAdminStorage.Repository.AcpEntityContextFactory())
                )).As<acpAdminStorage.Repository.IUserRepository>();

            builder.Register((c, p) => new acpAdminStorage.Repository.GeneralDataAccessRepository(
                new acpAdminStorage.Repository.AcpEntityContextFactory())).As<acpAdminStorage.Repository.IGeneralDataAccessRepository>();

            
            builder.Register((c, p) => new acpAdminStorage.Repository.TenantRepository(
                new acpAdminStorage.Repository.AcpEntityContextFactory())).As<IRepository<tenant>>();              

            builder.Register((c, p) => new acpAdminStorage.Repository.EnvironmentRepository(
                new acpAdminStorage.Repository.AcpEntityContextFactory())).As<IRepository<environment>>();

            builder.Register((c, p) => new acpAdminStorage.Repository.EnvironmentModuleRepository(
               new acpAdminStorage.Repository.AcpEntityContextFactory())).As<IEnvironmentModuleRepository>();

            builder.Register((c, p) => new acpAdminStorage.Repository.MenuRepository(
               new acpAdminStorage.Repository.AcpEntityContextFactory())).As<IMenuRepository>();

            builder.Register((c, p) => new acpAdminStorage.Repository.UserSlotWidgetRepository(
               new acpAdminStorage.Repository.AcpEntityContextFactory())).As<IUserSlotWidgetRepository>();

            // Metadata Repository
            builder.RegisterType<MetadataSchemaRepository>().As<IMetadataSchemaRepository>();
            builder.RegisterType<AcpDataTypeRepository>().As<IAcpDataTypeRepository>();
            builder.RegisterType<UIControlRepository>().As<IRepository<uiControl>>();
            builder.RegisterType<UserScheduleRepository>().As<IRepository<userSchedule>>();
            builder.RegisterType<ModuleGdConfigurationRepository>().As<IModuleGdConfigurationRepository>();
            
            builder.RegisterType<GdConnectionSettingRepository>().As<IGdConnectionSettingRepository>();
            builder.RegisterType<ProjectRepository>().As<IRepository<project>>();
           // builder.RegisterType<PublishRepository>().As<IPublishRepository>();
           // builder.RegisterType<DataFieldRepository>().As <IRepository<dataField>>();
            builder.RegisterType<TableConstraintRepository>().As<ITableConstraintRepository>();
            builder.RegisterType<OverruleUiControlSettingRepository>().As<IRepository<OverruleFieldControlSetting>>();
            builder.RegisterType<OverruleCustomPropertyRepository>().As<IOverruleCustomPropertyRepository>();            
            builder.RegisterType<HeaderTranslationRepository>().As<IHeaderTranslationRepository>();
            builder.RegisterType<PrimaryKeyConstraintRepository>().As<IRepository<primaryKeyConstraint>>();
            builder.RegisterType<ForeignKeyConstraintRepository>().As<IRepository<foreignKeyConstraint>>();
            builder.RegisterType<IndexConstraintRepository>().As<IRepository<indexConstraint>>();
            builder.RegisterType<PrimaryKeyFieldRepository>().As<IPrimaryKeyFieldRepository>();   
            builder.RegisterType<SchemaCustomPropertyRepository>().As<ISchemaCustomPropertyRepository>();
            builder.RegisterType<schemaDynamicButtonRepository>().As<IschemaDynamicButtonRepository>();
            builder.RegisterType<GdDynamicButtonRepository>().As<IRepository<gdDynamicButton>>();
            builder.RegisterType<GdAccessFilterViewRepository>().As<IGdAccessFilterViewRepository>();

            builder.RegisterType<QueryCreator>().As<Creator>();
            
            builder.RegisterType<DataFieldGroupRepository>().As<IDataFieldGroupRepository>();
            builder.RegisterType<ActionConfigurationRepository>().As<IActionConfigurationRepository>();
            builder.RegisterType<ActionConfigCustomPropertyRepository>().As<IActionConfigCustomPropertyRepository>();            
            builder.RegisterType<DbmsDataTypeRepository>().As<IDbmsDatatypeRepository>();
            builder.RegisterType<DbmsRepository>().As<IRepository<dbms>>();
            builder.RegisterType<ProjectAssetRepository>().As<IProjectAssetRepository>();            
            builder.RegisterType<DbmsFunctionRepository>().As<IRepository<dbmsFunction>>();
            builder.RegisterType<DataFieldCreator>().As<IDataFieldCreator>();
            builder.RegisterType<FkFieldReferenceRepository>().As<IFkFieldReferenceRepository>();            
            builder.RegisterType<MetadataAuditRepository>().As<IMetadataAuditRepository>();
            builder.RegisterType<DataFieldRepository>().As<IDataFieldRepository>();
            builder.RegisterType<QueryPartBuilder>().As<IQueryPartBuilder>();

            builder.RegisterType<WidgetRepository>().As<IWidgetRepository>();
            builder.RegisterType<ChartWidgetConfigRepository>().As<IChartWidgetConfigRepository>();
            builder.RegisterType<UserCredentialRepository>().As<IUserCredentialRepository>();

            builder.RegisterType<CustomPropertyFilterRepository>().As<IRepository<customPropertyFilter>>();
            builder.RegisterType<CustomPropertyDefinitionRepository>().As<IRepository<customPropertyDefinition>>();
            builder.RegisterType<CustomPropertyValueRepository>().As<IRepository<customPropertyValue>>();

            builder.RegisterType<MasterTableNameTranslationRepository>().As<IMasterTableNameTranslationRepository>();
            builder.RegisterType<FunctionScriptRepository>().As<IRepository<functionScript>>();
            builder.RegisterType<LinkManagerStoreRepository>().As<IRepository<linkManagerStore>>();  
            
        }

        protected virtual void registerPublishAndAuditRepositories()
        {
            builder.RegisterType<PublishingAuditRepository>().As<IPublishingAuditRepository>();
            builder.RegisterType<AuditingRepository>().As<IAuditingRepository>();
        }

        /// <summary>
        /// Resolve Type.
        /// </summary>
        /// <param name="type">Type to resolve.</param>
        /// <returns>Returns concrete implementation.</returns>  
        public object resolve(Type type)
        {
            using (var scope = container.BeginLifetimeScope())
            {
                return scope.Resolve(type);
            }
        }
    }
}