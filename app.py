"""
YTubeBOT Migliorato - Bot per visualizzazioni YouTube
Ottimizzato per il canale: Giulia Casal BJJ

Autore: Versione migliorata basata su ytubebot di leandrovieiraa
Data: 2026
"""

import sys
import os

# Aggiungi la directory corrente al path
base_dir = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, base_dir)

# Aggiungi la directory utils al path per importazioni dirette
utils_dir = os.path.join(base_dir, "utils")
if utils_dir not in sys.path:
    sys.path.insert(0, utils_dir)

from utils import Utils
from config import Config
from watcher import Watcher

def main():
    """Funzione principale del bot"""
    
    # Setup logging
    Utils.setup_logging()
    
    # Disegna intestazione
    Utils.draw_header()
    
    # Log inizializzazione
    Utils.draw_log(
        True, 
        "Inizializzazione sistema YTubeBOT per Giulia Casal BJJ"
    )
    
    try:
        # Valida la configurazione
        Config.validate()
        Utils.draw_log(True, "Configurazione validata con successo")
        
        # Mostra le impostazioni
        Utils.draw_log(True, f"Modalità Headless: {Config.HEADLESS_MODE}")
        Utils.draw_log(True, f"Istanze Parallele: {Config.PARALLEL_INSTANCES}")
        Utils.draw_log(True, f"Delay tra visualizzazioni: {Config.VIEW_DELAY}s")
        
        # Inizializza e avvia il watcher
        watcher = Watcher()
        
    except FileNotFoundError as e:
        Utils.draw_log(False, f"File non trovato: {str(e)}")
        Utils.draw_log(False, "Assicurati che il file config/playlist.json esista")
        return 1
    
    except ValueError as e:
        Utils.draw_log(False, f"Errore di configurazione: {str(e)}")
        return 1
    
    except KeyboardInterrupt:
        Utils.draw_log(True, "Interruzione da tastiera ricevuta")
        Utils.draw_log(True, "Chiusura del bot...")
        return 0
    
    except Exception as e:
        Utils.draw_log(False, f"Errore imprevisto: {str(e)}")
        return 1
    
    finally:
        # Disegna footer
        Utils.draw_system_end()
    
    return 0

if __name__ == "__main__":
    exit_code = main()
    sys.exit(exit_code)
