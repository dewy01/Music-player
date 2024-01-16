﻿using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using System.Text.Json;
using Spotify.logic;
using Spotify.view;
using Spotify.VIew;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Text.Json.Serialization;

namespace Spotify;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, IObserver
{
    public Biblioteka biblioteka;
    public Player player;
    public static MainWindow mainWindow;
    public Playlista playlist;
    public Utwor songToPlay;
    public Originator originator;
    public List<Memento> savedStates;
    public bool isPlaying;
    public MainWindow()
    {
        Playlista playProto = new Playlista("Template");
        biblioteka = Biblioteka.GetInstance();
        if (File.Exists("test.json"))
        {
            string jsonString = File.ReadAllText("test.json");
            // var deserialized = JsonConvert<Biblioteka>(jsonString);
            var deserialized = JsonConvert.DeserializeObject<Biblioteka>(jsonString);
            biblioteka = deserialized;
        }
        InitializeComponent();
        shuffle.Source = new BitmapImage(new Uri(@"/img/mix.png", UriKind.Relative));
        stop.Source = new BitmapImage(new Uri(@"/img/pause.png", UriKind.Relative));
        delete.Source = new BitmapImage(new Uri(@"/img/minus.png", UriKind.Relative));

        originator = new Originator();
        savedStates = new List<Memento>();
        player = Player.getInstance();
        isPlaying = false;
        UpdateUI();
    }

    public void save()
    {
        originator.State = JsonSerializer.Serialize<Biblioteka>(biblioteka, new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        });
        File.WriteAllText("test.json", originator.State);
        savedStates.Add(originator.SaveState());
    }

    private void CreateNewPlaylistButton_OnClick(object sender, RoutedEventArgs e)
    {
        AddPlaylist addPlaylist = new AddPlaylist(biblioteka);
        
        addPlaylist.Show();

    }


    private void moveToPlaylist(object sender, SelectionChangedEventArgs e)
    {
        if (playlistList.SelectedIndex >= 0)
        {

                songsList.SelectedIndex = -1;
                songsTitle.Text = biblioteka.getPlaylista(playlistList.SelectedIndex).getNazwa();
                songsList.Items.Clear();
                playlist = biblioteka.getPlaylista(playlistList.SelectedIndex);
                foreach (var i in playlist.getLista())
                {
                    songsList.Items.Add(i.getTytul() + " " + i.getAutor().getNazwisko());
                }
            
        }
    }

    private void loadSong(object sender, RoutedEventArgs e)
    {
        songToPlay = playlist.getUtwor(songsList.SelectedIndex);
    }

    private void CreateNewSongButton_OnClick(object sender, RoutedEventArgs e)
    {
        AddSong addSong = new AddSong(playlist, biblioteka);
        addSong.Closed += addSong_Closed;
        addSong.Show();
    }

    private void addSong_Closed(object sender, EventArgs e)
    {
        UpdateUI();
    }

    private void PlayButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (isPlaying == false)
        {
            if (songToPlay != null)
            {
                player.play(songToPlay.getSciezka());
                isPlaying = true;
                //playPauseImg.Source = "/../../../img/play.png";
                playPauseImg.Source = new BitmapImage(new Uri(@"/img/stop.png", UriKind.Relative));
            }
        }
        else
        {
            isPlaying = false;
            player.pause();
            playPauseImg.Source = new BitmapImage(new Uri(@"/img/play.png", UriKind.Relative));
        }

        UpdateUI();

    }

    private void PauseButton_OnClick(Object sender, RoutedEventArgs e)
    {
        player.stop();
    }

    private void RandomSongButton_OnClick(Object sender, RoutedEventArgs e)
    {
        if (isPlaying == false && playlist != null)
        {
            playlist.wymieszaj();
        }
    }

    private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        double newValue = e.NewValue;

        player.AdjustVolume(newValue);
    }

    private void DeleteSongButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (playlistList != null && songsList.SelectedIndex >= 0)
        {

            playlist.usunUtwor(songsList.SelectedIndex);

            UpdateUI();
        }
    }




    private void UpdateUI()
    {
        playlistList.Items.Clear();
        foreach (var i in biblioteka.getPlaylisty())
        {
            playlistList.Items.Add(i.getNazwa());
        }

        if (playlistList.SelectedIndex >= 0)
        {
            
            songsTitle.Text = biblioteka.getPlaylista(playlistList.SelectedIndex).getNazwa();
            songsList.Items.Clear();
            playlist = biblioteka.getPlaylista(playlistList.SelectedIndex);
            foreach (var i in playlist.getLista())
            {
                songsList.Items.Add(i.getTytul() + " " + i.getAutor().getNazwisko());
            }
        }
        save();

    }

    public void Update()
    {

        UpdateUI();
    }

}