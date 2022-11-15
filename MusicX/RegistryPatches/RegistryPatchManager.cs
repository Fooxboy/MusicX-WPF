using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicX.RegistryPatches
{
    public class RegistryPatchManager
    {
        private IEnumerable<IRegistryPatch> _patches;

        public RegistryPatchManager(IEnumerable<IRegistryPatch> patches)
        {
            _patches = patches;
        }

        public async Task Execute()
        {
            var needPatches = _patches.Where(p => p.CanRun());

            foreach(var patch in needPatches)
            {
                if (patch.RequiredAdmin) return; //todo: доделать

                patch.Run();
            }
        }
    }
}
