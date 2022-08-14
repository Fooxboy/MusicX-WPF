using MusicX.Core.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace MusicX.Tests
{
    public class VkTests
    {
        [Fact]
        public async Task GetAudioCatalogTest()
        {
            var vkSevice = new VkService(null);

            await vkSevice.SetTokenAsync("", null);

            var catalog = await vkSevice.GetAudioCatalogAsync();

            Assert.True(catalog.Catalog != null && catalog.Catalog.Sections.Count > 0);
        }

        [Fact]
        public async Task GetSectionTest()
        {
            var vkSevice = new VkService(null);

            await vkSevice.SetTokenAsync("", null);

            var catalog = await vkSevice.GetSectionAsync("PUldVA8NR0RzSVNUagdSRGpJUFQPBUdEfklTQgABWV9zX1FPGRZcRHxJBg5NTBILLxkGGBdJ");

            Assert.True(catalog.Audios.Count > 0);
        }

        [Fact]
        public async Task GetPlaylistTest()
        {
            var vkSevice = new VkService(null);

            await vkSevice.SetTokenAsync("", null);

            var plist = await vkSevice.GetPlaylistAsync(100, 84615054, "60192863dd2492b9f1", -143914468);

            Assert.True(plist.Playlist != null);
        }


        [Fact]
        public async Task AddAudioTest()
        {
            var vkSevice = new VkService(null);

            await vkSevice.SetTokenAsync("", null);

            await vkSevice.AudioAddAsync(106983262, -2001983262);

            Assert.True(true);
        }

        [Fact]
        public async Task GetArtistAudioTest()
        {
            var vkSevice = new VkService(null);

            await vkSevice.SetTokenAsync("", null);

            var res = await vkSevice.GetAudioArtistAsync("835651927344928174");

            Assert.True(true);
        }

        [Fact]
        public async Task GetBlockTest()
        {
            var vkSevice = new VkService(null);

            await vkSevice.SetTokenAsync("", null);

            var res = await vkSevice.GetBlockItemsAsync("PUlQVA8GR0R3W0tMF0YOBSkGGilWQRkHMgQbBRcYSV5kUVpGDQNdUnFTX1oXAUlcZA4RBllbGQNkR0tCFw5aSmRaXFQPBUdEdF1LTAYEU1FwX15OA0k");

            Assert.True(true);
        }

        [Fact]
        public async Task SearchTest()
        {
            var vkSevice = new VkService(null);

            await vkSevice.SetTokenAsync("", null);

            var res = await vkSevice.GetAudioSearchAsync("Монеточка");

            Assert.True(true);
        }

        [Fact]
        public async Task ReplaceBlockTest()
        {
            var vkSevice = new VkService(null);

            await vkSevice.SetTokenAsync("", null);

            var res = await vkSevice.ReplaceBlockAsync("PUkZGlRNBw81HzYfURZRS3dHS08XDllKZFpZVA8WNFRkR0tOFw5fU3NZUEMBDFJKZF5LTBdrWURqSV1UDwVHRHdeS0wEGElUcElTQgABWV9zX1FPSA/PUkaGUdANAQ_SVNAGRYbCicSBR9GQDQPIklTWwQYSV9kUVtaFwVbRHxJNkQXGEleZFFdQwAGUlNyU1BaFwFJXGQ0W1QZFl9EfFpFVAQBSVx3R0tEAxZRUnNeW08AAFNfOw#PUldVA8FR0RzSVNUWE00BzMPABlGawkfGRIMF0cWR0R_SVNHGRZTRHxfXEMHDV5SflIU");

            Assert.True(true);
        }

        [Fact]
        public async Task CreateGUID()
        {
            var g = Guid.NewGuid().GetHashCode();

            Assert.True(true);
        }

        [Fact]
        public async Task GetFile()
        {
             string PathInstall = AppDomain.CurrentDomain.BaseDirectory;

            var d = new DirectoryInfo(PathInstall);

            foreach(var item in d.GetFiles())
            {
                var n = item.Name;
            }

        }


        [Fact]
        public async Task SetCoverImage()
        {
            var vkSevice = new VkService(null);
            await vkSevice.SetTokenAsync("", null);

            await vkSevice.SetPlaylistCoverAsync(308764786, 13);
        }


    }
}
