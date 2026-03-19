import { useState, useEffect, useCallback } from 'react';
import { getAllAnime } from './services/api';
import SearchBar from './components/SearchBar';
import AnimeList from './components/AnimeList';
import AddAnimeModal from './components/AddAnimeModal';

const PAGE_SIZE = 20;

export default function App() {
  const [animes, setAnimes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [activeFilter, setActiveFilter] = useState('');
  const [selectedAnime, setSelectedAnime] = useState(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  const fetchAnimes = useCallback(async (currentPage = page) => {
    setLoading(true);
    try {
      const res = await getAllAnime(activeFilter || undefined, currentPage, PAGE_SIZE);
      setAnimes(res.data.data);
      setTotalPages(res.data.totalPages);
    } catch {
      // silencioso, la lista queda vacía
    } finally {
      setLoading(false);
    }
  }, [activeFilter, page]);

  useEffect(() => {
    setPage(1);
  }, [activeFilter]);

  useEffect(() => {
    fetchAnimes(page);
  }, [activeFilter, page]);

  const handleFilterChange = (filter) => {
    setActiveFilter(filter);
    setPage(1);
  };

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
          onFilterChange={handleFilterChange}
          onRefresh={() => fetchAnimes(page)}
          page={page}
          totalPages={totalPages}
          onPageChange={setPage}
        />
      </div>

      {selectedAnime && (
        <AddAnimeModal
          anime={selectedAnime}
          onClose={() => setSelectedAnime(null)}
          onAdded={() => fetchAnimes(page)}
        />
      )}
    </div>
  );
}
