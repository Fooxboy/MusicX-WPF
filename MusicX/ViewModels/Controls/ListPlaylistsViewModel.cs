using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.ViewModels.Controls
{
    public class ListPlaylistsViewModel:BaseViewModel
    {

        public List<Playlist> Playlists { get; set; }
    }
}
