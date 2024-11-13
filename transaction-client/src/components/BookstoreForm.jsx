import React, { useState, useEffect } from 'react';
import { getBooks, getClients, submitPurchaseRequest } from '../services/api';
import '../styles/BookstoreForm.css';

export default function BookstoreForm() {
    const [bookID, setBookID] = useState('');
    const [count, setCount] = useState(1);
    const [userID, setUserID] = useState('');
    const [pricePerPC, setPricePerPC] = useState(0);
    const [responseMessage, setResponseMessage] = useState('');
    const [books, setBooks] = useState([]);
    const [users, setUsers] = useState([]);

    // Funkcija za dohvaćanje podataka
    const fetchData = async () => {
        try {
            const booksData = await getBooks();
            const clientsData = await getClients();
            console.log("Knjige:", booksData);
            console.log("Klijenti:", clientsData);
            setBooks(booksData);
            setUsers(clientsData);
        } catch (error) {
            console.error("Error fetching data:", error);
        }
    };

    // Učitaj podatke pri prvom prikazu
    useEffect(() => {
        fetchData();
    }, []);

    const handleSubmit = async (event) => {
        event.preventDefault();
        const purchaseRequest = { userId: userID, bookId: bookID, quantity: count, pricePerPC };

        try {
            const result = await submitPurchaseRequest(purchaseRequest);
            setResponseMessage(result);

            // Ako je kupovina uspešna, automatski ponovo učitaj podatke sa servera
            if (result === "Kupovina uspešna") {
                await fetchData(); // Ponovo učitavanje svih podataka sa servera
            }
        } catch (error) {
            setResponseMessage("Kupovina nije uspela. Pokušajte ponovo.");
        }
    };

    return (
        <div className="transaction-form-container">
            <div className="transaction-form">
                <h2>Prodavnica knjiga</h2>
                <form onSubmit={handleSubmit}>
                    <label>
                        <i className="bi bi-person-fill" style={{ color: 'green' }}></i> Kupac:
                    </label>
                    <div className="input-with-icon">
                        <select onChange={(e) => setUserID(e.target.value)} required>
                            <option value="">Izaberi kupca</option>
                            {users.map(user => (
                                <option key={user.clientId} value={user.clientId}>
                                    {user.fullName} (Bankovni račun: ${user.accountBalance.toFixed(2)})
                                </option>
                            ))}
                        </select>
                    </div>

                    <label>
                        <i className="bi bi-book-fill" style={{ color: 'green' }}></i> Knjiga:
                    </label>
                    <div className="input-with-icon">
                        <select onChange={(e) => {
                            const selectedBook = books.find(book => book.bookId === e.target.value);
                            setBookID(selectedBook?.bookId || '');
                            setPricePerPC(selectedBook?.unitPrice || 0);
                        }} required>
                            <option value="">Izaberi knjigu</option>
                            {books.map(book => (
                                <option key={book.bookId} value={book.bookId}>
                                    {book.nameBook} (${book.unitPrice.toFixed(2)} - {book.quantity} na stanju)
                                </option>
                            ))}
                        </select>
                    </div>

                    <label>Količina:</label>
                    <input
                        type="number"
                        min="1"
                        max={books.find(book => book.bookId === bookID)?.quantity || 1}
                        value={count}
                        onChange={(e) => setCount(Number(e.target.value))}
                        required
                    />

                    <button type="submit">Kupi knjigu</button>
                </form>
                {responseMessage && <p className="response-message">{responseMessage}</p>}
            </div>
        </div>
    );
}
