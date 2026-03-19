import { useState, useEffect, useCallback } from 'react';
import { getAllAnime } from './services/api';
import SearchBar from './components/SearchBar';
import AnimeList from './components/AnimeList';
import AddAnimeModal from './components/AddAnimeModal';

export default function App() {
  const [animes, setAnimes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [activeFilter, setActiveFilter] = useState('');
  const [selectedAnime, setSelectedAnime] = useState(null);

  const fetchAnimes = useCallback(async () => {
    setLoading(true);
    try {
      const res = await getAllAnime(activeFilter || undefined);
      setAnimes(res.data);
    } catch {
      // silencioso, la lista queda vacía
    } finally {
      setLoading(false);
    }
  }, [activeFilter]);

  useEffect(() => {
    fetchAnimes();
  }, [fetchAnimes]);

  return (
    <div className="min-h-screen bg-zinc-950 text-white">
      <div className="max-w-6xl mx-auto px-4 py-8">
        <h1 className="text-3xl font-bold text-center mb-2">AniLog</h1>

        <div className="mb-8">
          <SearchBar onSelectAnime={setSelectedAnime} />
        </div>

        <AnimeList
          animes={animes}
          loading={loading}
          activeFilter={activeFilter}
          onFilterChange={setActiveFilter}
          onRefresh={fetchAnimes}
        />
      </div>

      {selectedAnime && (
        <AddAnimeModal
          anime={selectedAnime}
          onClose={() => setSelectedAnime(null)}
          onAdded={fetchAnimes}
        />
      )}
    </div>
  );
}
