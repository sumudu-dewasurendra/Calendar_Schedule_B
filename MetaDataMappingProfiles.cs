using acpAdminLogic.Mappers;
using acpAdminStorage.Models;
using acpCommon.Compressor;
using acpCommon.Context;
using acpCommon.Entity;
using acpCommon.Mappers;
using acpCommon.Odata;
using acpDtoModel.Models;
using acpDtoModel.Models.GeneralData;
using acpImportExport.Export;
using acpMetaDataLogic.Decoder;
using acpMetaDataLogic.Services;
using acpMetaDataStorage.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.OData.Query;

namespace acpMetaDataLogic.Mappers
{
    public class MetaDataMappingProfiles : Profile
    {
        public static List<tableConstraint> allTableConstrains = new List<tableConstraint>();
        public static void registerMappers()
        {
            Mapper.CreateMap<IEntity, IDto>().ConvertUsing<EntityToDtoTypeConverter>();
            Mapper.CreateMap<IDto, IEntity>().ConvertUsing<DtoToEntityTypeConverter>();

            Mapper.CreateMap<GdAcpDataTypeDto, acpDataType>();
            Mapper.CreateMap<acpDataType, GdAcpDataTypeDto>();

            Mapper.CreateMap<GdConnectionSettingDto, gdConnectionSetting>();
            Mapper.CreateMap<gdConnectionSetting, GdConnectionSettingDto>();

            Mapper.CreateMap<GdDbConnectionDto, gdDbConnection>()
                .ForMember(ev => ev.dbms, opts => opts.Ignore())
            .ForMember(ev => ev.encryptedPassword, opts => opts.MapFrom(e => e.userPassword));
            Mapper.CreateMap<gdDbConnection, GdDbConnectionDto>()
                .BeforeMap((s, d) =>
                {
                    AcpDataEntityTransformer.incrementedRefId = AcpDataEntityTransformer.incrementedRefId + 1;
                    d.refId = AcpDataEntityTransformer.incrementedRefId;

                })
            .ForMember(ev => ev.userPassword, opts => opts.MapFrom(e => e.encryptedPassword));

            Mapper.CreateMap<GdFileStreamConnectionDto, gdFileStreamConnection>();
            Mapper.CreateMap<gdFileStreamConnection, GdFileStreamConnectionDto>();

            Mapper.CreateMap<GdWebServiceConnectionDto, gdWebServiceConnection>();
            Mapper.CreateMap<gdWebServiceConnection, GdWebServiceConnectionDto>();

            Mapper.CreateMap<GdAssemblyConnectionDto, gdAssemblyConnection>();
            Mapper.CreateMap<gdAssemblyConnection, GdAssemblyConnectionDto>();

            Mapper.CreateMap<GdUIControlDto, uiControl>();
            Mapper.CreateMap<uiControl, GdUIControlDto>();           

            Mapper.CreateMap<GdSchemaDto, acpMetaData>()
                .ForMember(ev => ev.id, opts => opts.MapFrom(e => e.metadataSchemaId))
                .ForMember(ev => ev.dataFields, opts => opts.Ignore())
                .ForMember(ev => ev.isDeleted, opts => opts.MapFrom(e => e.isDeleted))
                .ForMember(ev => ev.defaultConnectionId, opts => opts.MapFrom(e => e.defaultConnectionId))               
                .ForMember(ev => ev.tableConstraints, opts => opts.Ignore())
                .ForMember(ev => ev.translationSource, opts => opts.MapFrom(e => e.recordTranslationSetting.translationSource))
                .ForMember(ev => ev.translationLocaleProperty, opt => opt.MapFrom(e => e.recordTranslationSetting.translationLocaleProperty));
              //  .ForMember(ev => ev.masterTable, opt => opt.MapFrom(e => e.masterTableTranslation));


            Mapper.CreateMap<acpMetaData, GdSchemaDto>()
                .BeforeMap((s, d) =>
                    {
                        d.foriegnKeyConstraints = new List<int>();
                        d.indexConstraints = new List<int>();
                        if (s.tableConstraints != null)
                        {
                            foreach (var item in s.tableConstraints)
                            {
                                if (item is primaryKeyConstraint)
                                    d.primaryKeyConstraint = AutoMapper.Mapper.Map<int>(item.id);

                                else if (item is foreignKeyConstraint)
                                {
                                    d.foriegnKeyConstraints.Add(AutoMapper.Mapper.Map<int>(item.id));
                                }

                                else if (item is indexConstraint)
                                {
                                    d.indexConstraints.Add(AutoMapper.Mapper.Map<int>(item.id));
                                }
                            }
                        }

                        d.fields = new List<int>();
                        if (s.dataFields != null)
                        {
                            foreach (var item in s.dataFields)
                            {
                                d.fields.Add(AutoMapper.Mapper.Map<int>(item.id));
                            }
                        }

                    })
                .ForMember(ev => ev.metadataSchemaId, opts => opts.MapFrom(e => e.id))
                .ForMember(ev => ev.isDeleted, opts => opts.MapFrom(e => e.isDeleted))
                .ForMember(ev => ev.defaultConnectionId, opts => opts.MapFrom(e => e.defaultConnectionId))                
                .ForMember(ev => ev.masterTableTranslation, opts => opts.MapFrom(e => e.masterTable))
                .AfterMap((s, d) =>
                {
                    d.recordTranslationSetting = new GdRecordTranslationSetting()
                        {
                            translationSource = s.translationSource,
                            translationLocaleProperty = s.translationLocaleProperty
                        };
                });

            Mapper.CreateMap<acpMetaDataTranslation, GdSchemaDto>()
              .BeforeMap((s, d) =>
                  {
                      if (s.masterTableName == null)
                      { d.masterTableTranslation = d.masterTable; }
                      else d.masterTableTranslation = s.masterTableName;
                  })
             .ForMember(ev => ev.entityId, opts => opts.Ignore());

            Mapper.CreateMap<GdSchemaFieldDto, dataField>()
                .ForMember(ev => ev.id, opts => opts.MapFrom(e => e.FieldId))
                .ForMember(ev => ev.metaDataId, opts => opts.MapFrom(e => e.MetadataSchemaID))
                .ForMember(ev => ev.acpDataTypeId, opts => opts.MapFrom(e => e.DataTypeId))
                .ForMember(ev => ev.defaultVal, opts => opts.MapFrom(e => e.DefaultValue))
                .ForMember(ev => ev.OverrulingControlSettingId, opts => opts.MapFrom(e => e.OverrulingControlSettingId));
            //.AfterMap((s, d) => { if (s.FieldOverruleControlSettings != null) d.OverrulingControlSettingId = s.FieldOverruleControlSettings.settingId; });

            Mapper.CreateMap<dataField, GdSchemaFieldDto>()
                .ForMember(ev => ev.FieldId, opts => opts.MapFrom(e => e.id))
                .ForMember(ev => ev.MetadataSchemaID, opts => opts.MapFrom(e => e.metaDataId))
                .ForMember(ev => ev.DataTypeId, opts => opts.MapFrom(e => e.acpDataTypeId))
                .ForMember(ev => ev.DefaultValue, opts => opts.MapFrom(e => e.defaultVal))
                .ForMember(ev => ev.OverrulingControlSettingId, opts => opts.MapFrom(e => e.OverrulingControlSettingId));

            Mapper.CreateMap<GdSchemaFieldGroupDto, dataFieldGroup>()
               .ForMember(ev => ev.id, opts => opts.MapFrom(e => e.GroupId))
               .ForMember(ev => ev.groupName, opts => opts.MapFrom(e => e.GroupName))
               .ForMember(ev => ev.groupSequenceNum, opts => opts.MapFrom(e => e.GroupSequenceNum))
               .ForMember(ev => ev.containerControlId, opts => opts.MapFrom(e => e.containerControlId))
               .ForMember(ev => ev.isExpanded, opts => opts.MapFrom(e => e.isExpanded));

            Mapper.CreateMap<dataFieldGroup, GdSchemaFieldGroupDto>()
               .ForMember(ev => ev.GroupId, opts => opts.MapFrom(e => e.id))
               .ForMember(ev => ev.GroupName, opts => opts.MapFrom(e => e.groupName))
               .ForMember(ev => ev.GroupSequenceNum, opts => opts.MapFrom(e => e.groupSequenceNum))
               .ForMember(ev => ev.containerControlId, opts => opts.MapFrom(e => e.containerControlId))
               .ForMember(ev => ev.isExpanded, opts => opts.MapFrom(e => e.isExpanded));

            Mapper.CreateMap<GdOverruleUiControlSettingDto, OverruleFieldControlSetting>()
               .ForMember(ev => ev.id, opts => opts.MapFrom(e => e.settingId))
               .ForMember(ev => ev.overruleCustomProperties, opts => opts.MapFrom(e => e.tags))
               .AfterMap((s, d) =>
               {
                   d.uiControlId = s.uiControlId;

                   if (d.overruleCustomProperties != null)
                   {
                       foreach (var customProperty in d.overruleCustomProperties)
                       {
                           customProperty.overruleSettingId = s.settingId;
                       }
                   }
               });

            Mapper.CreateMap<OverruleFieldControlSetting, GdOverruleUiControlSettingDto>()
                .ForMember(ev => ev.settingId, opts => opts.MapFrom(e => e.id))
                .ForMember(ev => ev.tags, opts => opts.MapFrom(e => e.overruleCustomProperties));

            Mapper.CreateMap<OverruleCustomPropertyDto, overruleCustomProperty>()
                .ForMember(ev => ev.propertyName, opts => opts.MapFrom(e => e.name))
                .ForMember(ev => ev.propertyValue, opts => opts.MapFrom(e => e.value));

            Mapper.CreateMap<overruleCustomProperty, OverruleCustomPropertyDto>()
                .ForMember(ev => ev.name, opts => opts.MapFrom(e => e.propertyName))
                .ForMember(ev => ev.value, opts => opts.MapFrom(e => e.propertyValue));

            Mapper.CreateMap<ActionConfigCustomPropertyDto, actionConfigCustomProperty>()
               .ForMember(ev => ev.id, opts => opts.MapFrom(e => e.id))
               .ForMember(ev => ev.propertyName, opts => opts.MapFrom(e => e.name))
               .ForMember(ev => ev.propertyValue, opts => opts.MapFrom(e => e.value));

            Mapper.CreateMap<actionConfigCustomProperty, ActionConfigCustomPropertyDto>()
                .ForMember(ev => ev.id, opts => opts.MapFrom(e => e.id))
                .ForMember(ev => ev.name, opts => opts.MapFrom(e => e.propertyName))
                .ForMember(ev => ev.value, opts => opts.MapFrom(e => e.propertyValue));

            Mapper.CreateMap<GdTableConstraintDto, tableConstraint>()
                .ForMember(ev => ev.id, opts => opts.MapFrom(e => e.constraintId))
                .ForMember(ev => ev.constrainName, opts => opts.MapFrom(e => e.constraintName))
                .ForMember(ev => ev.metaDataId, opts => opts.MapFrom(e => e.schemaId));
            //.Include <GdIndexConstraintDto,indexConstraint>()
            //.Include <GdForeignKeyConstraintDto, foreignKeyConstraint>()
            //.Include <GdPrimaryKeyConstraintDto,primaryKeyConstraint>();

            Mapper.CreateMap<tableConstraint, GdTableConstraintDto>()
                .ForMember(ev => ev.constraintId, opts => opts.MapFrom(e => e.id))
                .ForMember(ev => ev.constraintName, opts => opts.MapFrom(e => e.constrainName))
                .ForMember(ev => ev.schemaId, opts => opts.MapFrom(e => e.metaDataId));
            //.Include<indexConstraint, GdIndexConstraintDto>()
            //.Include<foreignKeyConstraint, GdForeignKeyConstraintDto>()
            //.Include<primaryKeyConstraint, GdPrimaryKeyConstraintDto>();

            Mapper.CreateMap<indexConstraint, GdIndexConstraintDto>()
                .ForMember(ev => ev.constraintId, opts => opts.MapFrom(e => e.id))
                .ForMember(ev => ev.constraintName, opts => opts.MapFrom(e => e.constrainName))
                .ForMember(ev => ev.schemaId, opts => opts.MapFrom(e => e.metaDataId))
                .ForMember(ev => ev.indexFieldId, opts => opts.MapFrom(e => e.indexFieldId))
                .ForMember(ev => ev.isUnique, opts => opts.MapFrom(e => e.isUnique));

            Mapper.CreateMap<GdIndexConstraintDto, indexConstraint>()
                .ForMember(ev => ev.id, opts => opts.MapFrom(e => e.constraintId))
                .ForMember(ev => ev.constrainName, opts => opts.MapFrom(e => e.constraintName))
                .ForMember(ev => ev.metaDataId, opts => opts.MapFrom(e => e.schemaId))
                .ForMember(ev => ev.indexFieldId, opts => opts.MapFrom(e => e.indexFieldId))
                .ForMember(ev => ev.isUnique, opts => opts.MapFrom(e => e.isUnique));

            Mapper.CreateMap<foreignKeyConstraint, GdForeignKeyConstraintDto>()
                .ForMember(ev => ev.constraintId, opts => opts.MapFrom(e => e.id))
                .ForMember(ev => ev.constraintName, opts => opts.MapFrom(e => e.constrainName))
                .ForMember(ev => ev.schemaId, opts => opts.MapFrom(e => e.metaDataId));

            Mapper.CreateMap<GdForeignKeyConstraintDto, foreignKeyConstraint>()
                .ForMember(ev => ev.id, opts => opts.MapFrom(e => e.constraintId))
                .ForMember(ev => ev.constrainName, opts => opts.MapFrom(e => e.constraintName))
                .ForMember(ev => ev.metaDataId, opts => opts.MapFrom(e => e.schemaId));

            Mapper.CreateMap<GdFkFieldReferenceDto, fkFieldReference>();

            Mapper.CreateMap<fkFieldReference, GdFkFieldReferenceDto>();

            Mapper.CreateMap<primaryKeyConstraint, GdPrimaryKeyConstraintDto>()
                .ForMember(ev => ev.constraintId, opts => opts.MapFrom(e => e.id))
                .ForMember(ev => ev.constraintName, opts => opts.MapFrom(e => e.constrainName))
                .ForMember(ev => ev.schemaId, opts => opts.MapFrom(e => e.metaDataId))
                .ForMember(ev => ev.primaryKeyFields, opts => opts.MapFrom(e => e.primaryKeyFields));

            Mapper.CreateMap<GdPrimaryKeyConstraintDto, primaryKeyConstraint>()
                .ForMember(ev => ev.id, opts => opts.MapFrom(e => e.constraintId))
                .ForMember(ev => ev.constrainName, opts => opts.MapFrom(e => e.constraintName))
                .ForMember(ev => ev.metaDataId, opts => opts.MapFrom(e => e.schemaId))
                .ForMember(ev => ev.primaryKeyFields, opts => opts.MapFrom(e => e.primaryKeyFields))
                .AfterMap((s, d) =>
                 {
                     foreach (var pkfield in d.primaryKeyFields)
                     {
                         pkfield.PrimaryKeyConstraintId = s.constraintId;
                     }
                 });

            Mapper.CreateMap<GdPrimaryKeyFieldDto, primaryKeyField>();

            Mapper.CreateMap<primaryKeyField, GdPrimaryKeyFieldDto>();

            Mapper.CreateMap<dataField, GdDataViewFieldDto>()
                .ForMember(ev => ev.fieldId, opts => opts.MapFrom(e => e.id))
                .ForMember(ev => ev.dataTypeId, opts => opts.MapFrom(e => e.acpDataTypeId))
                .ForMember(ev => ev.displayName, opts => opts.MapFrom(e => e.fieldName))
                .ForMember(ev => ev.overrulingControlSettingId, opts => opts.MapFrom(e => e.OverrulingControlSettingId))
                .AfterMap((s, d) =>
                {
                    d.uiControllerId = s.OverruleFieldControlSetting != null ? (int)s.OverruleFieldControlSetting.uiControlId : s.acpDataType.uiControlId;
                    d.fieldsEncoding = s.OverruleFieldControlSetting != null && !String.IsNullOrEmpty(s.OverruleFieldControlSetting.encodingFilter) ?
                        s.OverruleFieldControlSetting.encodingFilter : null;
                });

            Mapper.CreateMap<GdModuleConfigDto, moduleGeneralDataSchema>();
            Mapper.CreateMap<moduleGeneralDataSchema, GdModuleConfigDto>();

            Mapper.CreateMap<GdProjectDto, project>()
                .ForMember(ev => ev.id, opts => opts.MapFrom(e => e.projectId))
                .ForMember(ev => ev.publishedDateTime, opts => opts.MapFrom(e => e.publishedDateTime))
                .ForMember(ev => ev.projectStatus, opts => opts.MapFrom(e => e.projectStatus));

            Mapper.CreateMap<project, GdProjectDto>()
                .ForMember(ev => ev.projectId, opts => opts.MapFrom(e => e.id))
                .ForMember(ev => ev.publishedDateTime, opts => opts.MapFrom(e => e.publishedDateTime))
                .ForMember(ev => ev.projectStatus, opts => opts.MapFrom(e => e.projectStatus));

            Mapper.CreateMap<ProjectAssetDto, projectAsset>()
                .ForMember(ev => ev.Id, opts => opts.MapFrom(e => e.id))
                .ForMember(ev => ev.projectId, opts => opts.MapFrom(e => e.projectId))
                .ForMember(ev => ev.assetStatus, opts => opts.MapFrom(e => e.assetStatus))
                .AfterMap((s, d) =>
                {                    
                    if (s.record != null)
                    {
                        var binaryData = ObjectCodec.serialize((GdDataViewRecordDto)s.record);
                        var compressor = StreamCompressorCodec.getDefaultCompressor();
                        d.publishData = compressor.compress(binaryData);
                    }
                    else
                    {
                        d.publishData = null;
                    }                    
                });

            Mapper.CreateMap<projectAsset, ProjectAssetDto>()
                .ForMember(ev => ev.id, opts => opts.MapFrom(e => e.Id))
                .ForMember(ev => ev.projectId, opts => opts.MapFrom(e => e.projectId))
                .ForMember(ev => ev.assetStatus, opts => opts.MapFrom(e => e.assetStatus))
                .AfterMap((s, d) =>
                {
                    if(s.publishData != null)
                    {
                        var compressor = StreamCompressorCodec.getDefaultCompressor();
                        var decompressData = compressor.decompress(s.publishData);

                        var decodeObject = ObjectCodec.deserialize(decompressData);

                        if (decodeObject.GetType().Equals(typeof(GdDataViewRecordDto)))
                        {
                            d.record = (GdDataViewRecordDto)decodeObject;
                        } 
                    }
                    else
                    {
                        d.record = null;
                    }                   
                });

            Mapper.CreateMap<GdDynamicButtonDto, gdDynamicButton>();
            Mapper.CreateMap<gdDynamicButton, GdDynamicButtonDto>();

            Mapper.CreateMap<SchemaDynamicButtonDto, schemaDynamicButton>();
            Mapper.CreateMap<schemaDynamicButton, SchemaDynamicButtonDto>();

            Mapper.CreateMap<GdFieldHeaderTranslationDto, fieldHeaderTranslation>();
            Mapper.CreateMap<fieldHeaderTranslation, GdFieldHeaderTranslationDto>();

            Mapper.CreateMap<ActionConfigurationDto, actionConfiguration>()
                .ForMember(ev => ev.id, opts => opts.MapFrom(e => e.id))
                .ForMember(ev => ev.actionConfigCustomProperties, opts => opts.MapFrom(e => e.tags))
                 .AfterMap((s, d) =>
                 {
                     if (d.actionConfigCustomProperties != null)
                     {
                         foreach (var customProperty in d.actionConfigCustomProperties)
                         {
                             customProperty.actionConfigId = s.id;
                         }
                     }
                 });

            Mapper.CreateMap<actionConfiguration, ActionConfigurationDto>()
                .ForMember(ev => ev.id, opts => opts.MapFrom(e => e.id))
                .ForMember(ev => ev.tags, opts => opts.MapFrom(e => e.actionConfigCustomProperties));

            Mapper.CreateMap<NavigationActionConfigurationDto, navigationActionConfiguration>()
                .ForMember(ev => ev.actionConfigCustomProperties, opts => opts.MapFrom(e => e.tags))
                .AfterMap((s, d) =>
                   {
                       if (d.actionConfigCustomProperties != null)
                       {
                           foreach (var customProperty in d.actionConfigCustomProperties)
                           {
                               customProperty.actionConfigId = s.id;
                           }
                       }
                   });

            Mapper.CreateMap<navigationActionConfiguration, NavigationActionConfigurationDto>()
                .ForMember(ev => ev.tags, opts => opts.MapFrom(e => e.actionConfigCustomProperties));

            Mapper.CreateMap<CustomActionConfigurationDto, customActionConfiguration>()
                .ForMember(ev => ev.actionConfigCustomProperties, opts => opts.MapFrom(e => e.tags))
                .AfterMap((s, d) =>
                   {
                       if (d.actionConfigCustomProperties != null)
                       {
                           foreach (var customProperty in d.actionConfigCustomProperties)
                           {
                               customProperty.actionConfigId = s.id;
                           }
                       }
                   });

            Mapper.CreateMap<customActionConfiguration, CustomActionConfigurationDto>()
                .ForMember(ev => ev.tags, opts => opts.MapFrom(e => e.actionConfigCustomProperties));

            Mapper.CreateMap<SelectionDataLoadActionConfigurationDto, selectionDataLoadActionConfiguration>()
                .ForMember(ev => ev.actionConfigCustomProperties, opts => opts.MapFrom(e => e.tags))
                .AfterMap((s, d) =>
                {
                    if (d.actionConfigCustomProperties != null)
                    {
                        foreach (var customProperty in d.actionConfigCustomProperties)
                        {
                            customProperty.actionConfigId = s.id;
                        }
                    }
                });

            Mapper.CreateMap<selectionDataLoadActionConfiguration, SelectionDataLoadActionConfigurationDto>()
                .ForMember(ev => ev.tags, opts => opts.MapFrom(e => e.actionConfigCustomProperties));

            Mapper.CreateMap<GdDataLoadActionConfigurationDto, gdDataLoadActionConfiguration>()
                .ForMember(ev => ev.actionConfigCustomProperties, opts => opts.MapFrom(e => e.tags))
                .AfterMap((s, d) =>
                {
                    if (d.actionConfigCustomProperties != null)
                    {
                        foreach (var customProperty in d.actionConfigCustomProperties)
                        {
                            customProperty.actionConfigId = s.id;
                        }
                    }
                });

            Mapper.CreateMap<gdDataLoadActionConfiguration, GdDataLoadActionConfigurationDto>()
                .ForMember(ev => ev.tags, opts => opts.MapFrom(e => e.actionConfigCustomProperties));

            Mapper.CreateMap<DbmsDto, dbms>()
                .ForMember(ev => ev.dbmsType, opts => opts.MapFrom(e => e.dbmsType));

            Mapper.CreateMap<dbms, DbmsDto>()
                 .ForMember(ev => ev.dbmsType, opts => opts.MapFrom(e => e.dbmsType));

            Mapper.CreateMap<DbmsDataTypeDto, dbmsDataType>();
            Mapper.CreateMap<dbmsDataType, DbmsDataTypeDto>();

            Mapper.CreateMap<DbmsFunctionDto, dbmsFunction>();
            Mapper.CreateMap<dbmsFunction, DbmsFunctionDto>();

            Mapper.CreateMap<ComparatorRecordDto, GdDataViewRecordPublishDto>()
                .ForMember(ev => ev.schemaDefinitionId, opts => opts.MapFrom(e => e.schemaDefinitionId))
                .AfterMap((s, d) =>
                {
                    if (s.record != null)
                    {
                        d.records = new Dictionary<string, object>();

                        foreach (var field in s.record)
                        {
                            if (field.Value.newValue != null)
                            {
                                d.records.Add(field.Key, field.Value.newValue);
                            }
                            else
                            {
                                d.records.Add(field.Key, field.Value.oldValue);
                            }
                        }
                    }
                });

            Mapper.CreateMap<widget, WidgetDto>()
                .ForMember(ev => ev.widgetType, opts => opts.MapFrom(e => e.widgetType));
            Mapper.CreateMap<WidgetDto, widget>()
                 .ForMember(ev => ev.widgetType, opts => opts.MapFrom(e => e.widgetType));

            Mapper.CreateMap<customWidget, CustomWidgetDto>();
            Mapper.CreateMap<CustomWidgetDto, customWidget>();

            Mapper.CreateMap<chartWidget, ChartWidgetDto>();
            Mapper.CreateMap<ChartWidgetDto, chartWidget>();

            Mapper.CreateMap<user, UserDto>();
            Mapper.CreateMap<UserDto, user>();

            Mapper.CreateMap<userCredential, UserCredentialDto>();
            Mapper.CreateMap<UserCredentialDto, userCredential>();

            Mapper.CreateMap<userSchedule, UserScheduleDto>();
            Mapper.CreateMap<UserScheduleDto, userSchedule>();

            Mapper.CreateMap<ODataQueryOptions, QueryOptionDto>()
                .ForMember(ev => ev.expand, opts => opts.MapFrom(e => e.SelectExpand.RawExpand))
                .ForMember(ev => ev.inlineCount, opts => opts.MapFrom(e => e.InlineCount.RawValue))
                .ForMember(ev => ev.filter, opts => opts.MapFrom(e => e.Filter.RawValue))
                .ForMember(ev => ev.orderBy, opts => opts.MapFrom(e => e.OrderBy.RawValue))
                .ForMember(ev => ev.select, opts => opts.MapFrom(e => e.SelectExpand.RawSelect))
                .ForMember(ev => ev.skip, opts => opts.MapFrom(e => e.Skip.RawValue))
                .ForMember(ev => ev.top, opts => opts.MapFrom(e => e.Top.RawValue))
                .AfterMap((s, d) =>
                {
                    d.exclude = OdataCommonHelper.readCustomQueryOption(s, AcpVariableContext.ODATA_EXCLUDE_OPTION_NAME);

                    d.applyOptions = OdataCommonHelper.readCustomQueryOption(s, AcpVariableContext.ODATA_APPLY_OPTION_NAME);

                    d.urlQuery = s.Request.RequestUri.Query;                    
                });

            Mapper.CreateMap<SchemaCustomPropertyDto, schemaCustomProperty>()
                .ForMember(ev => ev.propertyName, opts => opts.MapFrom(e => e.name))
                .ForMember(ev => ev.propertyValue, opts => opts.MapFrom(e => e.value));

            Mapper.CreateMap<schemaCustomProperty, SchemaCustomPropertyDto>()
                .ForMember(ev => ev.name, opts => opts.MapFrom(e => e.propertyName))
                .ForMember(ev => ev.value, opts => opts.MapFrom(e => e.propertyValue));
            
            Mapper.CreateMap<CustomPropertyFilterDto, customPropertyFilter>();
            Mapper.CreateMap<customPropertyFilter, CustomPropertyFilterDto>();

            Mapper.CreateMap<CustomPropertyDefinitionDto, customPropertyDefinition>();
            Mapper.CreateMap<customPropertyDefinition, CustomPropertyDefinitionDto>();

            Mapper.CreateMap<CustomPropertyValueDto, customPropertyValue>();
            Mapper.CreateMap<customPropertyValue, CustomPropertyValueDto>();

            Mapper.CreateMap<GdSchemaTranslationDto, acpMetaDataTranslation>();
            Mapper.CreateMap<acpMetaDataTranslation, GdSchemaTranslationDto>();

            Mapper.CreateMap<GdAccessFilterViewDto, gdAccessFilterView>();
            Mapper.CreateMap<gdAccessFilterView, GdAccessFilterViewDto>();
            

            Mapper.CreateMap<GroupDto, group>()
               .ForMember(ev => ev.parentGroup, opts => opts.MapFrom(e => e.parentGroup))
               .ForMember(ev => ev.groupPreference, opts => opts.Ignore())
               .ForMember(ev => ev.tenant, opts => opts.Ignore());
           
            Mapper.CreateMap<group, GroupDto>()
                .BeforeMap((s, d) =>
                {
                    AcpDataEntityTransformer.incrementedRefId = AcpDataEntityTransformer.incrementedRefId + 1;
                    d.refId = AcpDataEntityTransformer.incrementedRefId;
                })
               .ForMember(ev => ev.parentGroup, opts => opts.MapFrom(e => e.parentGroup))
               .ForMember(ev => ev.groupPreferences, opts => opts.MapFrom(e => e.groupPreference));

            Mapper.CreateMap<tenant, TenantDto>()
                .BeforeMap((s, d) =>
                {
                    AcpDataEntityTransformer.incrementedRefId = AcpDataEntityTransformer.incrementedRefId + 1;
                    d.refId = AcpDataEntityTransformer.incrementedRefId;
                    d.share = true;
                });
            Mapper.CreateMap<TenantDto, tenant>();

            Mapper.CreateMap<EnvironmentDto, environment>();
            Mapper.CreateMap<environment, EnvironmentDto>()
                 .BeforeMap((s, d) =>
                 {
                     AcpDataEntityTransformer.incrementedRefId = AcpDataEntityTransformer.incrementedRefId + 1;
                     d.refId = AcpDataEntityTransformer.incrementedRefId;

                     object[] objArray = new object[1];
                     objArray[0] = s.defaultPrimarySourceId;

                     var dataCon = EntityLoader.getEntity(objArray, EntityTypeDto.DataSourceConnection);
                     d.defaultPrimarySource = AutoMapper.Mapper.Map<GdDbConnectionDto>(dataCon);
                 });

            Mapper.CreateMap<EnvironmentModuleDto, environmentModule>();
            Mapper.CreateMap<environmentModule, EnvironmentModuleDto>()
                .BeforeMap((s, d) =>
                    {
                        object[] objArray = new object[1];
                        objArray[0] = s.connectionId;

                        var dataCon = EntityLoader.getEntity(objArray, EntityTypeDto.DataSourceConnection);
                        d.dataSourceConnection = AutoMapper.Mapper.Map<GdDbConnectionDto>(dataCon);
                    })
                    .ForMember(ev => ev.dashboardSlot, opts => opts.MapFrom(e => e.slot));

            Mapper.CreateMap<SchemaConnectionMapDto, schemaConnectionMap>();
            Mapper.CreateMap<schemaConnectionMap, SchemaConnectionMapDto>()
                 .BeforeMap((s, d) =>
                 {// ToDO: checks EntityLoader code review comments.
                     object[] objArray = new object[1];
                     objArray[0] = s.connectionId;

                     var dataCon = EntityLoader.getEntity(objArray, EntityTypeDto.DataSourceConnection);
                     d.dataSourceConnection = AutoMapper.Mapper.Map<GdDbConnectionDto>(dataCon);
                 });

            Mapper.CreateMap<MenuConnectionMapDto, menuConnectionMap>();
            Mapper.CreateMap<menuConnectionMap, MenuConnectionMapDto>()
                 .BeforeMap((s, d) =>
                 {// ToDO: checks EntityLoader code review comments.
                     object[] objArray = new object[1];
                     objArray[0] = s.connectionId;

                     var dataCon = EntityLoader.getEntity(objArray, EntityTypeDto.DataSourceConnection);
                     d.dataSourceConnection = AutoMapper.Mapper.Map<GdDbConnectionDto>(dataCon);
                 });

            Mapper.CreateMap<module, ModuleDto>()
               .BeforeMap((s, d) =>
               {
                   AcpDataEntityTransformer.incrementedRefId = AcpDataEntityTransformer.incrementedRefId + 1;
                   d.refId = AcpDataEntityTransformer.incrementedRefId;
               })
                .ForMember(ev => ev.parentModule, opts => opts.MapFrom(e => e.parentModule));

            Mapper.CreateMap<ModuleDto, module>();

            Mapper.CreateMap<GdFunctionScriptDto, functionScript>();
            Mapper.CreateMap<functionScript, GdFunctionScriptDto>();

            Mapper.CreateMap<LinkManagerStoreDto, linkManagerStore>();
            Mapper.CreateMap<linkManagerStore, LinkManagerStoreDto>();

            Mapper.CreateMap<moduleMenu, ModuleMainMenuDto>()
               .ForMember(ev => ev.childMenus, opts => opts.MapFrom(e => e.moduleContentMenus));
            Mapper.CreateMap<ModuleMainMenuDto, moduleMenu>()
                .ForMember(ev => ev.moduleContentMenus, opts => opts.MapFrom(e => e.childMenus));

            Mapper.CreateMap<moduleContentMenu, ModuleContentMenuDto>()
                .ForMember(ev => ev.childMenus, opts => opts.MapFrom(e => e.moduleContentMenus));
            Mapper.CreateMap<ModuleContentMenuDto, moduleContentMenu>()
                .ForMember(ev => ev.moduleContentMenus, opts => opts.MapFrom(e => e.childMenus));

            Mapper.CreateMap<menuAction, MenuActionDto>();
            Mapper.CreateMap<MenuActionDto, menuAction>();

            Mapper.CreateMap<customAction, CustomActionDto>();
            Mapper.CreateMap<CustomActionDto, customAction>();

            Mapper.CreateMap<generalDataInvokeAction, GeneralDataInvokeActionDto>();
            Mapper.CreateMap<GeneralDataInvokeActionDto, generalDataInvokeAction>();

            Mapper.CreateMap<userSlotWidget, UserSlotWidgetDto>();
            Mapper.CreateMap<UserSlotWidgetDto, userSlotWidget>();
        }
    }

    /// <summary>
    /// Implements the generic type converter to convert Entity to Dto.
    /// </summary>
    public class EntityToDtoTypeConverter : TypeConverter<IEntity, IDto>
    {
        protected override IDto ConvertCore(IEntity source)
        {
            if (source is acpDataType) return Mapper.Map<GdAcpDataTypeDto>(source);
            else if (source is uiControl) return Mapper.Map<GdUIControlDto>(source);
            else if (source is acpMetaData) return Mapper.Map<GdSchemaDto>(source);
            else if (source is dataField) return Mapper.Map<GdSchemaFieldDto>(source);
            else if (source is OverruleFieldControlSetting) return Mapper.Map<GdOverruleUiControlSettingDto>(source);
            else if (source is overruleCustomProperty) return Mapper.Map<OverruleCustomPropertyDto>(source);
            else if (source is actionConfigCustomProperty) return Mapper.Map<ActionConfigCustomPropertyDto>(source);
            else if (source is indexConstraint) return Mapper.Map<GdIndexConstraintDto>(source);
            else if (source is foreignKeyConstraint) return Mapper.Map<GdForeignKeyConstraintDto>(source);
            else if (source is fkFieldReference) return Mapper.Map<GdFkFieldReferenceDto>(source);
            else if (source is primaryKeyConstraint) return Mapper.Map<GdPrimaryKeyConstraintDto>(source);
            else if (source is primaryKeyField) return Mapper.Map<GdPrimaryKeyFieldDto>(source);
            else if (source is gdDbConnection) return Mapper.Map<GdDbConnectionDto>(source);
            else if (source is gdFileStreamConnection) return Mapper.Map<GdFileStreamConnectionDto>(source);
            else if (source is gdWebServiceConnection) return Mapper.Map<GdWebServiceConnectionDto>(source);
            else if (source is gdAssemblyConnection) return Mapper.Map<GdAssemblyConnectionDto>(source);
            else if (source is moduleGeneralDataSchema) return Mapper.Map<GdModuleConfigDto>(source);
            else if (source is project) return Mapper.Map<GdProjectDto>(source);
            else if (source is projectAsset) return Mapper.Map<ProjectAssetDto>(source);
            else if (source is fieldHeaderTranslation) return Mapper.Map<GdFieldHeaderTranslationDto>(source);
            else if (source is dataFieldGroup) return Mapper.Map<GdSchemaFieldGroupDto>(source);
            else if (source is navigationActionConfiguration) return Mapper.Map<NavigationActionConfigurationDto>(source);
            else if (source is customActionConfiguration) return Mapper.Map<CustomActionConfigurationDto>(source);
            else if (source is selectionDataLoadActionConfiguration) return Mapper.Map<SelectionDataLoadActionConfigurationDto>(source);
            else if (source is gdDataLoadActionConfiguration) return Mapper.Map<GdDataLoadActionConfigurationDto>(source);
            else if (source is dbms) return Mapper.Map<DbmsDto>(source);
            else if (source is dbmsDataType) return Mapper.Map<DbmsDataTypeDto>(source);
            else if (source is dbmsFunction) return Mapper.Map<DbmsFunctionDto>(source);
            else if (source is chartWidget) return Mapper.Map<ChartWidgetDto>(source);
            else if (source is customWidget) return Mapper.Map<CustomWidgetDto>(source);
            else if (source is user) return Mapper.Map<UserDto>(source);
            else if (source is userCredential) return Mapper.Map<UserCredentialDto>(source);
            else if (source is schemaCustomProperty) return Mapper.Map<SchemaCustomPropertyDto>(source);
            else if (source is customPropertyFilter) return Mapper.Map<CustomPropertyFilterDto>(source);
            else if (source is customPropertyDefinition) return Mapper.Map<CustomPropertyDefinitionDto>(source);
            else if (source is customPropertyValue) return Mapper.Map<CustomPropertyValueDto>(source);
            else if (source is acpMetaDataTranslation) return Mapper.Map<GdSchemaTranslationDto>(source);
            else if (source is gdDynamicButton) return Mapper.Map<GdDynamicButtonDto>(source);
            else if (source is schemaDynamicButton) return Mapper.Map<SchemaDynamicButtonDto>(source);

            else if (source is gdAccessFilterView) return Mapper.Map<GdAccessFilterViewDto>(source);
            //else if (source is ODataQueryOptions) return Mapper.Map<QueryOptionDto>(source);


            else if (source is group) return Mapper.Map<GroupDto>(source);
            else if (source is tenant) return Mapper.Map<TenantDto>(source);
            else if (source is environment) return Mapper.Map<EnvironmentDto>(source);
            else if (source is environmentModule) return Mapper.Map<EnvironmentModuleDto>(source);
            else if (source is schemaConnectionMap) return Mapper.Map<SchemaConnectionMapDto>(source);
            else if (source is module) return Mapper.Map<ModuleDto>(source);
            else if (source is functionScript) return Mapper.Map<GdFunctionScriptDto>(source);
            else if (source is linkManagerStore) return Mapper.Map<LinkManagerStoreDto>(source);
            else if (source is menuConnectionMap) return Mapper.Map<MenuConnectionMapDto>(source);
            else if (source is moduleMenu) return Mapper.Map<ModuleMainMenuDto>(source);
            else if (source is moduleContentMenu) return Mapper.Map<ModuleContentMenuDto>(source);
            else if (source is customAction) return Mapper.Map<CustomActionDto>(source);
            else if (source is generalDataInvokeAction) return Mapper.Map<GeneralDataInvokeActionDto>(source);
            else if (source is userSlotWidget) return Mapper.Map<UserSlotWidgetDto>(source);
            else if (source is userSchedule) return Mapper.Map<UserScheduleDto>(source);
            return null;
        }
    }

    /// <summary>
    /// Implements the generic type converter to convert Dto to Entity.
    /// </summary>
    public class DtoToEntityTypeConverter : TypeConverter<IDto, IEntity>
    {
        protected override IEntity ConvertCore(IDto source)
        {
            if (source is GdAcpDataTypeDto) return Mapper.Map<acpDataType>(source);
            else if (source is GdUIControlDto) return Mapper.Map<uiControl>(source);
            else if (source is GdSchemaDto) return Mapper.Map<acpMetaData>(source);
            else if (source is GdSchemaFieldDto) return Mapper.Map<dataField>(source);
            else if (source is GdOverruleUiControlSettingDto) return Mapper.Map<OverruleFieldControlSetting>(source);
            else if (source is OverruleCustomPropertyDto) return Mapper.Map<overruleCustomProperty>(source);
            else if (source is ActionConfigCustomPropertyDto) return Mapper.Map<actionConfigCustomProperty>(source);
            else if (source is GdIndexConstraintDto) return Mapper.Map<indexConstraint>(source);
            else if (source is GdForeignKeyConstraintDto) return Mapper.Map<foreignKeyConstraint>(source);
            else if (source is GdFkFieldReferenceDto) return Mapper.Map<fkFieldReference>(source);
            else if (source is GdPrimaryKeyConstraintDto) return Mapper.Map<primaryKeyConstraint>(source);
            else if (source is GdPrimaryKeyFieldDto) return Mapper.Map<primaryKeyField>(source);
            else if (source is GdDbConnectionDto) return Mapper.Map<gdDbConnection>(source);
            else if (source is GdFileStreamConnectionDto) return Mapper.Map<gdFileStreamConnection>(source);
            else if (source is GdWebServiceConnectionDto) return Mapper.Map<gdWebServiceConnection>(source);
            else if (source is GdAssemblyConnectionDto) return Mapper.Map<gdAssemblyConnection>(source);
            else if (source is GdModuleConfigDto) return Mapper.Map<moduleGeneralDataSchema>(source);
            else if (source is GdProjectDto) return Mapper.Map<project>(source);
            else if (source is ProjectAssetDto) return Mapper.Map<projectAsset>(source);
            else if (source is GdFieldHeaderTranslationDto) return Mapper.Map<fieldHeaderTranslation>(source);
            else if (source is GdSchemaFieldGroupDto) return Mapper.Map<dataFieldGroup>(source);
            else if (source is NavigationActionConfigurationDto) return Mapper.Map<navigationActionConfiguration>(source);
            else if (source is CustomActionConfigurationDto) return Mapper.Map<customActionConfiguration>(source);
            else if (source is SelectionDataLoadActionConfigurationDto) return Mapper.Map<selectionDataLoadActionConfiguration>(source);
            else if (source is GdDataLoadActionConfigurationDto) return Mapper.Map<gdDataLoadActionConfiguration>(source);
            else if (source is DbmsDto) return Mapper.Map<dbms>(source);
            else if (source is DbmsDataTypeDto) return Mapper.Map<dbmsDataType>(source);
            else if (source is DbmsFunctionDto) return Mapper.Map<dbmsFunction>(source);
            else if (source is ChartWidgetDto) return Mapper.Map<chartWidget>(source);
            else if (source is CustomWidgetDto) return Mapper.Map<customWidget>(source);
            else if (source is UserDto) return Mapper.Map<user>(source);
            else if (source is UserCredentialDto) return Mapper.Map<userCredential>(source);
            else if (source is SchemaCustomPropertyDto) return Mapper.Map<schemaCustomProperty>(source);
            else if (source is CustomPropertyFilterDto) return Mapper.Map<customPropertyFilter>(source);
            else if (source is CustomPropertyDefinitionDto) return Mapper.Map<customPropertyDefinition>(source);
            else if (source is CustomPropertyValueDto) return Mapper.Map<customPropertyValue>(source);
            else if (source is GdSchemaTranslationDto) return Mapper.Map<acpMetaDataTranslation>(source);
            else if (source is GdDynamicButtonDto) return Mapper.Map<gdDynamicButton>(source);
            else if (source is SchemaDynamicButtonDto) return Mapper.Map<schemaDynamicButton>(source);
            else if (source is GroupDto) return Mapper.Map<group>(source);
            else if (source is TenantDto) return Mapper.Map<tenant>(source);
            else if (source is EnvironmentDto) return Mapper.Map<environment>(source);
            else if (source is EnvironmentModuleDto) return Mapper.Map<environmentModule>(source);
            else if (source is SchemaConnectionMapDto) return Mapper.Map<schemaConnectionMap>(source);
            else if (source is ModuleDto) return Mapper.Map<module>(source);
            else if (source is GdAccessFilterViewDto) return Mapper.Map<gdAccessFilterView>(source);
            else if (source is LinkManagerStoreDto) return Mapper.Map<linkManagerStore>(source);
            else if (source is MenuConnectionMapDto) return Mapper.Map<menuConnectionMap>(source);
            else if (source is ModuleMainMenuDto) return Mapper.Map<moduleMenu>(source);
            else if (source is ModuleContentMenuDto) return Mapper.Map<moduleContentMenu>(source);
            else if (source is CustomActionDto) return Mapper.Map<customAction>(source);
            else if (source is GeneralDataInvokeActionDto) return Mapper.Map<generalDataInvokeAction>(source);
            else if (source is UserSlotWidgetDto) return Mapper.Map<userSlotWidget>(source);
            else if (source is UserScheduleDto) return Mapper.Map<userSchedule>(source);
            return null;
        }
    }
}