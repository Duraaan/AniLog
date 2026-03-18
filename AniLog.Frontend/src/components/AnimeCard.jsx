import { useState } from 'react';
import { updateAnime, deleteAnime } from '../services/api';

const STATUS_LABELS = {
  Watching: 'Viendo',
  Completed: 'Completado',
  Dropped: 'Abandonado',
  PlanToWatch: 'Planeo ver',
};

const STATUS_COLORS = {
  Watching: 'bg-blue-500',
  Completed: 'bg-green-500',
  Dropped: 'bg-red-500',
  PlanToWatch: 'bg-zinc-500',
};

const STATUS_OPTIONS = ['Watching', 'Completed', 'Dropped', 'PlanToWatch'];

export default function AnimeCard({ anime, onRefresh }) {
  const [editing, setEditing] = useState(false);
  const [myScore, setMyScore] = useState(anime.myScore);
  const [myStatus, setMyStatus] = useState(anime.myStatus);
  const [episodesWatched, setEpisodesWatched] = useState(anime.episodesWatched);
  const [myNotes, setMyNotes] = useState(anime.myNotes || '');
  const [loading, setLoading] = useState(false);

  const handleUpdate = async () => {
    setLoading(true);
    try {
      await updateAnime(anime.id, {
        myScore: parseFloat(myScore),
        myStatus,
        episodesWatched: parseInt(episodesWatched),
        myNotes: myNotes || null,
      });
      setEditing(false);
      onRefresh();
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async () => {
    if (!confirm(`¿Eliminar "${anime.title}" de tu lista?`)) return;
    await deleteAnime(anime.id);
    onRefresh();
  };

  return (
    <div className="bg-zinc-800 border border-zinc-700 rounded-xl overflow-hidden flex flex-col">
      <img
        src={anime.imageUrl}
        alt={anime.title}
        className="w-full h-48 object-cover"
      />
      <div className="p-3 flex flex-col gap-2 flex-1">
        <div>
          <h3 className="text-white font-semibold text-sm leading-tight">{anime.title}</h3>
          {anime.genres && (
            <p className="text-zinc-500 text-xs mt-0.5">{anime.genres}</p>
          )}
        </div>

        <div className="flex items-center gap-2 flex-wrap">
          <span className={`text-xs text-white px-2 py-0.5 rounded-full ${STATUS_COLORS[anime.myStatus]}`}>
            {STATUS_LABELS[anime.myStatus]}
          </span>
          <span className="text-zinc-400 text-xs">MAL: {anime.malScore ?? '—'}</span>
        </div>

        {!editing ? (
          <>
            <div className="text-xs text-zinc-300 space-y-0.5">
              <p>Mi score: <span className="text-white font-medium">{anime.myScore}</span></p>
              <p>Eps: <span className="text-white">{anime.episodesWatched}/{anime.episodes ?? '?'}</span></p>
              {anime.myNotes && <p className="text-zinc-400 italic">"{anime.myNotes}"</p>}
            </div>
            <div className="flex gap-2 mt-auto pt-2">
              <button
                onClick={() => setEditing(true)}
                className="flex-1 py-1 text-xs rounded-lg bg-zinc-700 text-white hover:bg-zinc-600 transition"
              >
                Editar
              </button>
              <button
                onClick={handleDelete}
                className="flex-1 py-1 text-xs rounded-lg bg-red-900/50 text-red-400 hover:bg-red-900 transition"
              >
                Eliminar
              </button>
            </div>
          </>
        ) : (
          <div className="flex flex-col gap-2">
            <select
              value={myStatus}
              onChange={(e) => setMyStatus(e.target.value)}
              className="w-full px-2 py-1 text-xs bg-zinc-700 border border-zinc-600 rounded text-white focus:outline-none"
            >
              {STATUS_OPTIONS.map((s) => (
                <option key={s} value={s}>{STATUS_LABELS[s]}</option>
              ))}
            </select>
            <div className="flex gap-2">
              <input
                type="number"
                min="0"
                max="10"
                step="0.5"
                value={myScore}
                onChange={(e) => setMyScore(e.target.value)}
                placeholder="Score"
                className="w-1/2 px-2 py-1 text-xs bg-zinc-700 border border-zinc-600 rounded text-white focus:outline-none"
              />
              <input
                type="number"
                min="0"
                value={episodesWatched}
                onChange={(e) => setEpisodesWatched(e.target.value)}
                placeholder="Eps vistos"
                className="w-1/2 px-2 py-1 text-xs bg-zinc-700 border border-zinc-600 rounded text-white focus:outline-none"
              />
            </div>
            <textarea
              value={myNotes}
              onChange={(e) => setMyNotes(e.target.value)}
              placeholder="Notas..."
              rows={2}
              className="w-full px-2 py-1 text-xs bg-zinc-700 border border-zinc-600 rounded text-white focus:outline-none resize-none"
            />
            <div className="flex gap-2">
              <button
                onClick={() => setEditing(false)}
                className="flex-1 py-1 text-xs rounded-lg bg-zinc-700 text-white hover:bg-zinc-600 transition"
              >
                Cancelar
              </button>
              <button
                onClick={handleUpdate}
                disabled={loading}
                className="flex-1 py-1 text-xs rounded-lg bg-purple-600 text-white hover:bg-purple-500 transition disabled:opacity-50"
              >
                {loading ? '...' : 'Guardar'}
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
