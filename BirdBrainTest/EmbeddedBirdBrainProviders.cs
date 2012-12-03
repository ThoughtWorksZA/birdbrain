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

    class EmbeddedBirdBrainExtendedMembershipProvider : BirdBrainExtendedMembershipProvider
    {
        public EmbeddedBirdBrainExtendedMembershipProvider() : base(new EmbeddedBirdBrainMembershipProvider())
        {
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
