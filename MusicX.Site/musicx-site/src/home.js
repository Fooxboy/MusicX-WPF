import React from "react";
import image from './images/image-one.png'

class Home extends React.Component {
    render() {
        return(
            <div>
                <nav className="uk-navbar-container uk-sticky uk-navbar-transparent" uk-navbar>
                    <div className="uk-navbar-left">
                        <ul className="uk-navbar-nav">
                            <li className="uk-active"><a href="#">Music X</a></li>
                            <li>
                                <a href="#">Скачать</a>
                            </li>
                            <li><a href="#">Брр</a></li>
                        </ul>
                    </div>
                </nav>


                <div className="uk-panel">
                    <img src={image} className="left-image uk-align-left uk-margin-remove-adjacent" alt="image one"/>
                    <div>
                        <p className="title-app">
                            Music X
                        </p>

                        <p className="subtitle-app">
                            Музыкальный плеер ВКонтакте для Windows 11 и Windows 10
                        </p>

                        <div>
                               <a className="uk-button uk-button-secondary" href="#">
                                    <div>
                                        <p className="download-button">Скачать</p>
                                    </div>
                                </a>
                          
                            <a className="uk-button uk-button-secondary telegram-button" href="#">Телеграм</a>
                        </div>
                    </div>
                </div>

                <div className="uk-panel">
                    <img src={image} className="right-image uk-align-right uk-margin-remove-adjacent" alt="image one"/>
                    <div className="uk-panel">
                        <p className="desc-title">Рекомендации</p>
                        <p className="desc-subtitle">Раздел "Обзор" содержит все самые популярные и трендовые песни и альбомы всей музыкальной библиотеки ВКонтакте. Вы точно тут найдете что послушать!</p>
                    </div>
                </div>

                <div className="uk-panel">
                    <img src={image} className="left-image uk-align-left uk-margin-remove-adjacent" alt="image one"/>
                    <div className="uk-panel">
                        <p className="desc-title">Специальные подборки</p>
                        <p className="desc-subtitle">Это подобранные алгоритмами специально для Вас 5 плейлистов, которые постоянно обновляются, подстраиваясь под Ваш музыкальный вкус. А подборки от редакции помогут узнать новые жанры и стили!</p>
                    </div>
                </div>

                <div className="uk-panel">
                    <img src={image} className="right-image uk-align-right uk-margin-remove-adjacent" alt="image one"/>
                    <div className="uk-panel">
                        <p className="desc-title">Ваша музыка рядом</p>
                        <p className="desc-subtitle">Все Ваши плейлисты и треки хранятся в разделе "Музыка"</p>
                    </div>
                </div>

                <div className="uk-panel">
                    <img src={image} className="left-image uk-align-left uk-margin-remove-adjacent" alt="image one"/>
                    <div className="uk-panel">
                        <p className="desc-title">Подкасты</p>
                        <p className="desc-subtitle">Если Вам надоела музыка, Вы всегда сможете послушать прекрасных дикторов с их интереснешими историями!</p>
                    </div>
                </div>

                <div className="uk-panel">
                    <img src={image} className="right-image uk-align-right uk-margin-remove-adjacent" alt="image one"/>
                    <div className="uk-panel">
                        <p className="desc-title">Поиск</p>
                        <p className="desc-subtitle">Найдите все Ваши любимые треки, плейлисты, любимых артитов в одном запросе.</p>
                    </div>
                </div>
               
            </div>
        )
    }
}

export default Home;

