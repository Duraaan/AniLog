import { useState } from 'react';
import { addAnime } from '../services/api';

const STATUS_OPTIONS = ['Watching', 'Completed', 'Dropped', 'PlanToWatch'];
const STATUS_LABELS = {
  Watching: 'Viendo',
  Completed: 'Completado',
  Dropped: 'Abandonado',
  PlanToWatch: 'Planeo ver',
};

export default function AddAnimeModal({ anime, onClose, onAdded }) {
  const [myStatus, setMyStatus] = useState('PlanToWatch');
  const [myScore, setMyScore] = useState(0);
  const [episodesWatched, setEpisodesWatched] = useState(0);
  const [myNotes, setMyNotes] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      await addAnime({
        malId: anime.malId,
        myStatus,
        myScore: parseFloat(myScore),
        episodesWatched: parseInt(episodesWatched),
        myNotes: myNotes || null,
      });
      onAdded();
      onClose();
    } catch (e) {
      if (e.response?.status === 409)
        setError('Este anime ya está en tu lista.');
      else if (e.response?.status === 503)
        setError('No se pudo conectar a Jikan. Intenta en unos segundos.');
      else
        setError('Ocurrió un error. Intenta de nuevo.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black/70 flex items-center justify-center z-50 p-4">
      <div className="bg-zinc-900 border border-zinc-700 rounded-xl w-full max-w-md">
        <div className="flex gap-4 p-4 border-b border-zinc-700">
          <img
            src={anime.imageUrl}
            alt={anime.title}
            className="w-16 h-24 object-cover rounded"
          />
          <div className="flex flex-col justify-center">
            <h2 className="text-white font-semibold text-lg leading-tight">
              {anime.titleEnglish || anime.title}
            </h2>
            {anime.titleEnglish && (
              <p className="text-zinc-400 text-sm">{anime.title}</p>
            )}
            <p className="text-zinc-500 text-xs mt-1">
              {anime.episodes ? `${anime.episodes} eps` : 'Eps desconocidos'} · MAL {anime.score ?? '—'}
            </p>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="p-4 flex flex-col gap-3">
          <div>
            <label className="text-zinc-400 text-sm">Estado</label>
            <select
              value={myStatus}
              onChange={(e) => setMyStatus(e.target.value)}
              className="w-full mt-1 px-3 py-2 bg-zinc-800 border border-zinc-700 rounded-lg text-white focus:outline-none focus:border-purple-500"
            >
              {STATUS_OPTIONS.map((s) => (
                <option key={s} value={s}>{STATUS_LABELS[s]}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="text-zinc-400 text-sm">Mi score (0–10)</label>
            <input
              type="number"
              min="0"
              max="10"
              step="0.5"
              value={myScore}
              onChange={(e) => setMyScore(e.target.value)}
              className="w-full mt-1 px-3 py-2 bg-zinc-800 border border-zinc-700 rounded-lg text-white focus:outline-none focus:border-purple-500"
            />
          </div>

          <div>
            <label className="text-zinc-400 text-sm">Episodios vistos</label>
            <input
              type="number"
              min="0"
              value={episodesWatched}
              onChange={(e) => setEpisodesWatched(e.target.value)}
              className="w-full mt-1 px-3 py-2 bg-zinc-800 border border-zinc-700 rounded-lg text-white focus:outline-none focus:border-purple-500"
            />
          </div>

          <div>
            <label className="text-zinc-400 text-sm">Notas (opcional)</label>
            <textarea
              value={myNotes}
              onChange={(e) => setMyNotes(e.target.value)}
              rows={2}
              className="w-full mt-1 px-3 py-2 bg-zinc-800 border border-zinc-700 rounded-lg text-white focus:outline-none focus:border-purple-500 resize-none"
            />
          </div>

          {error && <p className="text-red-400 text-sm">{error}</p>}

          <div className="flex gap-2 pt-1">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 py-2 rounded-lg bg-zinc-700 text-white hover:bg-zinc-600 transition"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={loading}
              className="flex-1 py-2 rounded-lg bg-purple-600 text-white hover:bg-purple-500 transition disabled:opacity-50"
            >
              {loading ? 'Agregando...' : 'Agregar'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
