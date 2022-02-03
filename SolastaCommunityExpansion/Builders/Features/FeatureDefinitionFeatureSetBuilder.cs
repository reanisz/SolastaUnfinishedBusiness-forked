﻿using System;
using SolastaModApi;
using SolastaModApi.Extensions;

namespace SolastaCommunityExpansion.Builders.Features
{
    public class FeatureDefinitionFeatureSetBuilder : BaseDefinitionBuilder<FeatureDefinitionFeatureSet>
    {
        public FeatureDefinitionFeatureSetBuilder(FeatureDefinitionFeatureSet original, string name, string guid, GuiPresentation guiPresentation)
            : base(original, name, guid)
        {
            Definition.SetGuiPresentation(guiPresentation);
        }

        public FeatureDefinitionFeatureSetBuilder(string name, string guid)
            : base(name, guid)
        {
        }

        public FeatureDefinitionFeatureSetBuilder(string name, Guid namespaceGuid, string category = null)
            : base(name, namespaceGuid, category)
        {
        }

        public FeatureDefinitionFeatureSetBuilder(FeatureDefinitionFeatureSet original, string name, string guid)
            : base(original, name, guid)
        {
        }

        public FeatureDefinitionFeatureSetBuilder(FeatureDefinitionFeatureSet original, string name, Guid namespaceGuid, string category = null)
            : base(original, name, namespaceGuid, category)
        {
        }

        public FeatureDefinitionFeatureSetBuilder ClearFeatures()
        {
            Definition.FeatureSet.Clear();
            return this;
        }

        public FeatureDefinitionFeatureSetBuilder AddFeature(FeatureDefinition featureDefinition)
        {
            Definition.FeatureSet.Add(featureDefinition);
            return this;
        }

        public FeatureDefinitionFeatureSetBuilder SetMode(FeatureDefinitionFeatureSet.FeatureSetMode mode)
        {
            Definition.SetMode(mode);
            return this;
        }

        public FeatureDefinitionFeatureSetBuilder SetUniqueChoices(bool uniqueChoice)
        {
            Definition.SetUniqueChoices(uniqueChoice);
            return this;
        }
    }
}
