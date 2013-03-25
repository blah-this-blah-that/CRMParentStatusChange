using System;
using System.ServiceModel;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using System.Xml.Linq;

namespace ParentStatusChangeGenericPlugin
{
    public class EntityChangeState : IPlugin
    {
        string unsecureConfig = string.Empty;

        public EntityChangeState(string unsecure, string secure)
        {
            unsecureConfig = unsecure;
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService traceService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            Common.WriteToTrace(traceService, "Started post status change Plugin execution");

            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            EntityReference entity = null;

            ParentEntity parentEntity = new ParentEntity();
            List<ChildEntity> lstChildEntity = new List<ChildEntity>();

            PopulateParentAndChildEntities(parentEntity, lstChildEntity, unsecureConfig);

            if (context.InputParameters.Contains(Constants.ENTITYMONIKER) && context.InputParameters[Constants.ENTITYMONIKER] is EntityReference)
            {
                entity = (EntityReference)context.InputParameters[Constants.ENTITYMONIKER];

                if (context.PrimaryEntityName.ToLower() != parentEntity.ParentEntityName.ToLower())
                {
                    Common.WriteToTrace(traceService, "Exiting the plugin, the plugin is expected to execute for " + parentEntity.ParentEntityName + " entity, not for the " + entity.LogicalName + " entity.");
                    return;
                }
            }

            else
            {
                Common.WriteToTrace(traceService, "Exiting the plugin, the plugin is expected to execute for " + parentEntity.ParentEntityName + " entity, not for the " + entity.LogicalName + " entity11.");
                return;
            }

            try
            {
                if ((context.MessageName.ToLower().Equals(Constants.SETSTATEDYNAMICENTITY) || context.MessageName.ToLower().Equals(Constants.SETSTATE)) && context.InputParameters.Contains(Constants.ENTITYMONIKER) && context.InputParameters[Constants.ENTITYMONIKER] is EntityReference)
                {
                    int status = ((OptionSetValue)context.InputParameters[Constants.STATE]).Value;

                    foreach (var childEntity in lstChildEntity)
                    {
                        EntityCollection children = RetrieveChildren(service, entity, childEntity);

                        foreach (Entity child in children.Entities)
                        {
                            var setState = new SetStateRequest();

                            setState.EntityMoniker = new EntityReference()
                            {
                                Id = child.Id,
                                LogicalName = child.LogicalName
                            };

                            if (status == parentEntity.ParentInactiveStatecode)
                            {
                                setState.State = new OptionSetValue();
                                setState.State.Value = childEntity.ChildInactiveStatecode;
                                setState.Status = new OptionSetValue();
                                setState.Status.Value = childEntity.ChildInactiveStatuscode;

                                service.Execute(setState);
                            }

                            else if (status == parentEntity.ParentActiveStatecode)
                            {
                                setState.State = new OptionSetValue();
                                setState.State.Value = childEntity.ChildActiveStatecode;
                                setState.Status = new OptionSetValue();
                                setState.Status.Value = childEntity.ChildActiveStatuscode;

                                service.Execute(setState);
                            }
                        }
                    }

                    Common.WriteToTrace(traceService, "Child records status changed successfully");
                }
                else
                {
                    Common.WriteToTrace(traceService, "Exiting the plugin, the plugin is expected to execute for " + parentEntity.ParentEntityName + " entity, not for the " + entity.LogicalName + " entityElse.");
                }
            }
            catch (Exception ex)
            {
                Common.WriteToTrace(traceService, "An exception occured: " + ex.Message);
                throw ex;
            }
        }

        private static EntityCollection RetrieveChildren(IOrganizationService service, EntityReference entity, ChildEntity childEntity)
        {
            QueryExpression query = new QueryExpression();
            query.EntityName = childEntity.ChildEntityName;
            query.ColumnSet = new ColumnSet();
            query.ColumnSet.AddColumn(childEntity.PrimaryColumnName);

            ConditionExpression expr = new ConditionExpression();
            expr.AttributeName = childEntity.LookupSchemaName;
            expr.Operator = ConditionOperator.Equal;
            expr.Values.Add(entity.Id);
            query.Criteria.AddCondition(expr);

            EntityCollection children = service.RetrieveMultiple(query);
            return children;
        }

        private void PopulateParentAndChildEntities(ParentEntity parentEntity, List<ChildEntity> lstChildEntities, string unsecureConfig)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(unsecureConfig)))
            {
                reader.ReadToFollowing("ParentEntity");
                reader.MoveToAttribute("ParentEntityLogicalName");
                parentEntity.ParentEntityName = reader.Value;

                reader.MoveToAttribute("ParentActiveStateCode");
                parentEntity.ParentActiveStatecode = Convert.ToInt32(reader.Value);

                reader.MoveToAttribute("ParentInactiveStateCode");
                parentEntity.ParentInactiveStatecode = Convert.ToInt32(reader.Value);

                XDocument xmlDoc = XDocument.Parse(unsecureConfig);

                foreach (var childEntity in xmlDoc.Descendants("ChildEntity"))
                {
                    ChildEntity child = new ChildEntity();
                    child.ChildEntityName = childEntity.Attribute("ChildEntityLogicalName").Value;
                    child.PrimaryColumnName = childEntity.Attribute("PrimaryColumn").Value;
                    child.LookupSchemaName = childEntity.Attribute("LookupSchemaName").Value;
                    child.ChildActiveStatecode = Convert.ToInt32(childEntity.Attribute("ChildActiveStateCode").Value);
                    child.ChildInactiveStatecode = Convert.ToInt32(childEntity.Attribute("ChildInactiveStateCode").Value);
                    child.ChildActiveStatuscode = Convert.ToInt32(childEntity.Attribute("ChildActiveStatusCode").Value);
                    child.ChildInactiveStatuscode = Convert.ToInt32(childEntity.Attribute("ChildInactiveStatusCode").Value);

                    lstChildEntities.Add(child);
                };
            }
        }
    }
}
