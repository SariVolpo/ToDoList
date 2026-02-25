import React, { useEffect, useState } from 'react';
import service from './service.js';
import Login from './Login.js';
import { jwtDecode } from "jwt-decode";

function App() {
  const [newTodo, setNewTodo] = useState("");
  const [todos, setTodos] = useState([]);
  const [token, setToken] = useState(localStorage.getItem("token"));
  const [loggedInUser, setLoggedInUser] = useState("");


  const handleLogin = () => {
    setToken(localStorage.getItem("token"));
  };

  const handleLogout = () => {
    localStorage.removeItem("token");
    setToken(null);
  };

  async function getTodos() {
    if (token) {
      const todos = await service.getTasks();
      setTodos(todos);
    }
  }

  async function createTodo(e) {
    e.preventDefault();
    if (!newTodo.trim()) return;

    await service.addTask(newTodo);
    setNewTodo("");
    await getTodos();
  }

  async function updateCompleted(todo, isComplete) {
    await service.setComplete(todo.idItems, isComplete);
    await getTodos();
  }

  async function deleteTodo(idItems) {
    await service.deleteTask(idItems);
    await getTodos();
  }

  useEffect(() => {
    if (token) {
      try {
        const decoded = jwtDecode(token);
        setLoggedInUser(decoded.unique_name || "אורח");
      } catch (e) {
        console.error("Error decoding token", e);
      }
      getTodos();
    }
  }, [token]);

  if (!token) {
    return <Login onLogin={handleLogin} />;
  }

  return (
    <section className="todoapp">
      <div className="top-management-bar">
        <div className="user-info">
          <span>שלום, <strong>{loggedInUser}</strong></span>
        </div>
        <button className="logout-btn" onClick={handleLogout}>
          התנתקות
        </button>
      </div>
      <header className="header">
        <h1>ToDo List</h1>
        <form onSubmit={createTodo}>
          <input
            className="new-todo"
            placeholder="Well, let's take on the day"
            value={newTodo}
            onChange={(e) => setNewTodo(e.target.value)}
          />
        </form>
      </header>
      <section className="main" style={{ display: "block" }}>
        <ul className="todo-list">
          {todos.map(todo => {
            return (
              <li className={todo.isComplete ? "completed" : ""} key={todo.idItems}>
                <div className="view">
                  <input
                    className="toggle"
                    type="checkbox"
                    checked={todo.isComplete || false}
                    onChange={(e) => updateCompleted(todo, e.target.checked)}
                  />
                  <label>{todo.name}</label>
                  <button
                    className="destroy"
                    onClick={() => deleteTodo(todo.idItems)}
                  ></button>
                </div>
              </li>
            );
          })}
        </ul>
      </section>
    </section >
  );
}

export default App;