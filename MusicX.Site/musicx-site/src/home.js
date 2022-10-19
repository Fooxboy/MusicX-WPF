import React from "react";
import home from './images/home.png';
import explore from './images/explore.png';
import playlists from './images/playlists.png';
import vkmix from './images/vkmix.png';
import search from './images/search.png';
import downloads from './images/downloads.png';

class Home extends React.Component {
    render() {
        return(
            <div>
                <nav className="uk-navbar-container uk-sticky uk-navbar-transparent" uk-navbar>
                    <div className="uk-navbar-left">
                        <ul className="uk-navbar-nav">
                            <li className="uk-active"><a href="#">Music X</a></li>
                           
                            <li><a href="#opportunities">Возможности</a></li>
                            <li>
                                <a href="#download">Скачать</a>
                            </li>
                        </ul>
                    </div>
                </nav>


                <div className="uk-panel">
                    <img src={home} className="left-image uk-align-left uk-margin-remove-adjacent uk-border-rounded uk-box-shadow-xlarge" alt="image one"/>
                    <div>
                        <p className="title-app">
                            Music X
                        </p>

                        <p className="subtitle-app">
                            Музыкальный плеер <b>ВКонтакте</b> для Windows 11 и Windows 10
                        </p>

                        <div>
                               <a className="download-button" target="_blank" href="https://fooxboy.blob.core.windows.net/musicx/MusicXSetup.exe">
                                    Скачать
                                </a>
                          
                            <a className="telegram-button" href="https://t.me/MusicXPlayer">Телеграм</a>
                        </div>
                    </div>
                </div>

                <div id="opportunities" className="uk-panel">
                    <div>
                        <p className="category-title category-title-margin">Основные возможности</p>
                    </div>
                </div>

                <div className="uk-panel hiden">
                    <img src={explore} className="right-image uk-align-right uk-margin-remove-adjacent uk-border-rounded uk-box-shadow-xlarge" alt="image one"/>
                    <div >
                        <p className="desc-title">Рекомендации</p>
                        <p className="desc-subtitle">Раздел "Обзор" содержит все самые популярные и трендовые песни и альбомы всей музыкальной библиотеки ВКонтакте. Вы точно тут найдете что послушать!</p>
                    </div>
                </div>


               <div className="seporator-div"></div>

                <div className="uk-panel">
                    <img src={playlists} className="left-image uk-align-left uk-margin-remove-adjacent uk-border-rounded uk-box-shadow-xlarge" alt="image one"/>
                    <div>
                        <p className="desc-title">Специальные подборки</p>
                        <p className="desc-subtitle">Это подобранные алгоритмами специально для Вас 5 плейлистов, которые постоянно обновляются, подстраиваясь под Ваш музыкальный вкус. А подборки от редакции помогут узнать новые жанры и стили!</p>
                    </div>
                </div>

               <div className="seporator-div"></div>


                <div className="uk-panel  hiden">
                    <img src={search} className="right-image uk-align-right uk-margin-remove-adjacent uk-border-rounded uk-box-shadow-xlarge" alt="image one"/>
                    <div >
                        <p className="desc-title">Поиск</p>
                        <p className="desc-subtitle">Найдите все Ваши любимые треки, плейлисты, любимых артитов в одном запросе.</p>
                    </div>
                </div>

               <div className="seporator-div"></div>


                <div className="uk-panel">
                    <img src={vkmix} className="left-image uk-align-left uk-margin-remove-adjacent uk-border-rounded uk-box-shadow-xlarge" alt="image one"/>
                    <div >
                        <p className="desc-title">VK Mix</p>
                        <p className="desc-subtitle">Бесконечное радио по Вашим любимым исполнителям, жарнам или любимым трекам</p>
                    </div>
                </div>

                <div className="uk-panel  hiden">
                    <img src={downloads} className="right-image uk-align-right uk-margin-remove-adjacent uk-border-rounded uk-box-shadow-xlarge" alt="image one"/>
                    <div >
                        <p className="desc-title">Загрузка музыки</p>
                        <p className="desc-subtitle">Если Вам необходимо скачать свою библиотку музыки ВКонтакте на компьютер в формате .mp3, то Music X поможет вам в этом!</p>
                    </div>
                </div>

               <div className="seporator-div"></div>


                <div id="download" className="uk-panel block-dark">
                    <div className="uk-card uk-card-body">
                        <p className="category-title">
                            И ещё много чего разного!
                        </p>

                        <p className="category-desc">
                            Попробуйте сами все возможности загрузив Music X для Windows 11, 10
                        </p>

                        <a className="download-button" target="_blank" href="https://fooxboy.blob.core.windows.net/musicx/MusicXSetup.exe">
                            Скачать
                        </a>
                    </div>
                </div>
               
            </div>
        )
    }
}

export default Home;

