# EntitiesMigration Plugin to Copy Sitecore Commerce Entities Between Environments

*by* Sergey Yatsenko, July 17, 2019

## Overview

Copying content between environments is relatively simple in Sitecore CMS as it allows us to create content packages or serialize its content items into files natively or via third-party tools, such as Unicorn or TDS. With Sitecore Commerce content migration is more complicated because Commerce its support for content migration is a bit limited as of this moment, so we had to come up with some creative ways to export/import data in our projects, but it usually required writing some custom code.

The approach and patterns we used were more or less the same in all cases:

1. Export Commerce items of given type from source environment into a file, usually in JSON format
2. Import above file into the target environment via custom API

Sitecore Commerce provides some APIs out of the box, which allows into export/import catalogs or certain items via Postman and when this works - great, but more often than not this was not sufficient in our use cases.

I created the Entity Migration Plugin to address the most common migration needs. Migration scenarios and requirements came from my latest projects, but implementation is new, built from the ground up and designed to work for anybody.

## How Entity Migration Plugin Works

Entity Migration is a two-step process using the above-described approach where entities of each kind are exported into a file on the source environment and then from that file imported into the target environment. Entity Migration Plugin needs to be included in your Sitecore Commerce solution and then compiled and deployed to both source and target environments. Hopefully, this will work as-is, but if not then it can be a good starting point and can be modified as needed - all source code is provided in [this git repository.](https://github.com/sergyatsenko/Plugin.Sync.Commerce.EntitiesMigrationPlugin)

![Entities Migration Plugin Process Flow](https://cdn.xcentium.com/-/media/images/blog-images/entities-migration-plugin/entity-migration-plugin.ashx?la=en&hash=EC204760ABACDC613FD6CAE2FAE45736231A66C5&vs=1&d=20191218T004534Z)

## Using Entity Migration Plugin

**Word of caution: Please remember to test this process and backup data on your environments prior to using this plugin. I'm sharing this code as-is, it worked for me and I hope it'll help others to save time and effort, but as always, things can go wrong, so please, be careful and don't hold me responsible if something breaks.**

### SUPPORTED ENTITY TYPES

The following Entity Types can be migrated with EntityMigrationPlugin:

- Catalog
- Category
- SellableItem
- PriceBook
- PriceCard
- PromotionBook
- Promotion
- ComposerTemplate

### STEP 1: EXPORT ENTITIES FROM SOURCE ENVIRONMENT

For each kind of entity, you need to migrate call ExportEntities() API on the source environment, passing the catalogName and entityType parameters as needed. The value of entityType needs to match one of the above-mentioned types. Repeat this process for each catalog and each entity type you need. If you need to migrate Habitat_Master catalog for example then call ExportEntities() API three times, passing "Catalog", "Category" and then "SellableItem" as entityType. To save the output to file in Postman click little down arrow on the right side of the "Send" button then choose "Send and Download". The below screenshot shows how to export all Commerce Category items from "Habitat_Master" Catalog.

![Export Entities Call in Postman](https://cdn.xcentium.com/-/media/images/blog-images/entities-migration-plugin/export-entities.ashx?h=405&w=801&la=en&hash=B1FBDB725CE20E69F268FF2244F554DCCDEC2067&vs=1&d=20191218T004611Z)

Step 2: Import Entities into Target Environment

For each kind of imported entity call ImportEntities API, passing entities export files generated on step 1 as a parameter. No other parameters needed as entity type and catalog name are saved in the export file itself.

One thing to keep in mind - import entities in the correct order as Commerce Catalog entities can depend on their parents, so when migrating Catalog items then migrate them in the following order:

1. Catalog
2. Category
3. SellableItems

![Import Entities Call in Postman](https://cdn.xcentium.com/-/media/images/blog-images/entities-migration-plugin/import-entities.ashx?h=297&w=800&la=en&hash=61852AC7E40AE41C24F038A8C9ACB8126B171280&vs=1&d=20191218T004643Z)
