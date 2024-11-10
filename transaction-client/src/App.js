import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import BookstoreForm from './components/BookstoreForm';

function App() {
  return (
    <Router>
      <div className="App">
        <Routes>
          <Route path="/" element={<BookstoreForm />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;
