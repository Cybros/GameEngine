#include "CoreEngine.h"

CoreEngine::CoreEngine(Display* display, Game* game ,RenderingEngine* renderingEngine)
{
    m_running = false;
    this->m_game = game;
    this->renderingEngine = renderingEngine;
    this->m_display = display;
    //game->getRootObject()->setEngine(this);
    renderingEngine->init(m_display);
}
void CoreEngine::run()
{
    if(!m_running )
        return;
    if(!m_display->isRunning())
        stop();
    m_display->Clear(0.0f, 0.0f, 0.0f, 1.0f);
    m_display->update();
    renderingEngine->render(m_game->getRootObject());
    m_game->update(4.0f);
    m_display->SwapBuffers();
}
CoreEngine::~CoreEngine()
{
    //dtor
}
