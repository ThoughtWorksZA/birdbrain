﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BirdBrain;
using Raven.Client.Embedded;

namespace BirdBrainTest
{
    class EmbeddedBirdBrainMembershipProvider : BirdBrainMembershipProvider
    {
        protected override void InitializeDocumentStore()
        {
            DocumentStore = new EmbeddableDocumentStore
            {
                ConnectionStringName = BirdBrainRoleProvider.ConnectionStringName,
            };
            DocumentStore.Initialize();
        }
    }

    class EmbeddedBirdBrainRoleProvider : BirdBrainRoleProvider
    {
        protected override void InitializeDocumentStore()
        {
            DocumentStore = new EmbeddableDocumentStore
            {
                ConnectionStringName = ConnectionStringName,
            };
            DocumentStore.Initialize();
        }
    }
}
