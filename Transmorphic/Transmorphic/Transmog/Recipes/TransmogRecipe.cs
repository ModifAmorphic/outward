using ModifAmorphic.Outward.Extensions;
using ModifAmorphic.Outward.Modules.Crafting;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ModifAmorphic.Outward.Transmorphic.Transmog.Recipes
{
    internal class TransmogRecipe : CustomRecipe
    {
        [SerializeField]
        public int VisualItemID;

        public string RecipeName => this.GetPrivateField<Recipe, string>("m_name");
    }
}
