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

    useEffect(() => {
        async function fetchData() {
            try {
                const booksData = await getBooks();
                const clientsData = await getClients();
                setBooks(booksData);
                setUsers(clientsData);
            } catch (error) {
                console.error("Error fetching data:", error);
            }
        }
        fetchData();
    }, []);
    
    const handleSubmit = async (event) => {
        event.preventDefault();
        const purchaseRequest = { userId: userID, bookId: bookID, quantity: count, pricePerPC };

        try {
            const result = await submitPurchaseRequest(purchaseRequest);
            setResponseMessage(result);
        } catch (error) {
            setResponseMessage("Purchase failed. Please try again.");
        }
    };

    return (
        <div className="transaction-form-container">
            <div className="transaction-form">
                <h2>Prodavnica knjiga</h2>
                <form onSubmit={handleSubmit}>
                    <label>Kupac:</label>
                    <div className="input-with-icon">
                        <i className="bi bi-person-fill"></i>
                        <select onChange={(e) => setUserID(e.target.value)} required>
                            <option value="">Izaberi kupca</option>
                            {users.map(user => (
                                <option key={user.clientId} value={user.clientId}>
                                    {user.fullName} (Bankovni račun: ${user.accountBalance.toFixed(2)})
                                </option>
                            ))}
                        </select>
                    </div>

                    <label>Knjiga:</label>
                    <div className="input-with-icon">
                        <i className="bi bi-book-fill"></i>
                        <select onChange={(e) => {
                            const selectedBook = books.find(book => book.productId === e.target.value);
                            setBookID(selectedBook?.productId || '');
                            setPricePerPC(selectedBook?.unitPrice || 0);
                        }} required>
                            <option value="">Izaberi knjigu</option>
                            {books.map(book => (
                                <option key={book.productId} value={book.productId}>
                                    {book.name} (${book.unitPrice.toFixed(2)} - {book.stockQuantity} na stanju)
                                </option>
                            ))}
                        </select>
                    </div>

                    <label>Količina:</label>
                    <input
                        type="number"
                        min="1"
                        max={books.find(book => book.productId === bookID)?.stockQuantity || 1}
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
