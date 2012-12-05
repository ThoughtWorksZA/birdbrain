using BirdBrain;
using Raven.Client.Embedded;

namespace BirdBrainTest
{
    class EmbeddedBirdBrainMembershipProvider : BirdBrainMembershipProvider
    {
        protected override void InitializeDocumentStore(string connectionStringName)
        {
            DocumentStore = new EmbeddableDocumentStore
            {
                ConnectionStringName = connectionStringName
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
        protected override void InitializeDocumentStore(string connectionStringName)
        {
            DocumentStore = new EmbeddableDocumentStore
            {
                ConnectionStringName = connectionStringName
            };
            DocumentStore.Initialize();
        }
    }
}
