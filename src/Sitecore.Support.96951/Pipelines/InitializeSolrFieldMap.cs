namespace Sitecore.Support.Pipelines
{
  using Sitecore.Collections;
  using Sitecore.Configuration;
  using Sitecore.ContentSearch;
  using Sitecore.ContentSearch.Diagnostics;
  using Sitecore.ContentSearch.SolrProvider;
  using Sitecore.Data;
  using Sitecore.Data.Engines;
  using Sitecore.Data.Templates;
  using Sitecore.Pipelines;
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public class InitializeSolrFieldMap
  {
    public void Process(PipelineArgs args)
    {
      foreach (ISearchIndex index in SolrContentSearchManager.Indexes)
      {
        SolrSearchIndex solrSearchIndex = index as SolrSearchIndex;
        if (solrSearchIndex != null)
        {
          SolrFieldMap fieldMap = solrSearchIndex.Configuration.FieldMap as SolrFieldMap;
          if (fieldMap != null)
          {
            if (solrSearchIndex.Crawlers.Count == 0)
            {
              CrawlingLog.Log.Warn(string.Format("{0}: {1} index crawler cannot be resolved. Skipping.", (object)this.GetType(), (object)solrSearchIndex.Name), (Exception)null);
            }
            else
            {
              IProviderCrawler providerCrawler = solrSearchIndex.Crawlers.FirstOrDefault<IProviderCrawler>((Func<IProviderCrawler, bool>)(c => c is SitecoreItemCrawler));
              if (providerCrawler == null)
              {
                CrawlingLog.Log.Warn(string.Format("{0}: {1} index crawler is not of SitecoreItemCrawler type. Skipping.", (object)this.GetType(), (object)solrSearchIndex.Name), (Exception)null);
              }
              else
              {
                foreach (KeyValuePair<ID, Template> template in (SafeDictionary<ID, Template>)new TemplateEngine(Factory.GetDatabase((providerCrawler as SitecoreItemCrawler).Database)).GetTemplates())
                {
                  foreach (TemplateField field in template.Value.GetFields())
                  {
                    if (field.Name.Contains<char>(' '))
                    {
                      string fieldName = field.Name.Replace(' ', '_');
                      if (fieldMap.GetFieldConfiguration(fieldName) == null)
                      {
                        SolrSearchFieldConfiguration configurationByFieldTypeName = fieldMap.GetFieldConfigurationByFieldTypeName(field.TypeKey) as SolrSearchFieldConfiguration;
                        fieldMap.AddFieldByFieldName(fieldName, configurationByFieldTypeName);
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }
    }
  }
}
