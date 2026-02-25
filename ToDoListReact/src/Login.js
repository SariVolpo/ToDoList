import React, { useState } from 'react';
import service from './service';

function Login({ onLogin }) {
    const [isRegister, setIsRegister] = useState(false);
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');

    const handleSubmit = async (e) => {
        e.preventDefault(); 
        setError(''); 
        
        try {
            if (isRegister) {
                await service.register(username, password);
                setError("נרשמת בהצלחה! עכשיו אפשר להתחבר.");
                setIsRegister(false);
                setUsername(''); 
                setPassword('');
            } else {
                await service.login(username, password);
                onLogin(); 
            }
        } catch (err) {
            console.error("Login/Register error:", err);
            const message = err.response?.data?.message || err.response?.data || "אופס! משהו השתבש. בדקו את הפרטים.";
            setError(message);
        }
    };

    return (
        <div className="login-wrapper">
            <div className="login-card">
                <h2>{isRegister ? 'הרשמה למערכת' : 'כניסה למערכת'}</h2>
                <form onSubmit={handleSubmit} className="login-form">
                    <div>
                        <label>שם משתמש</label>
                        <input type="text" value={username} onChange={(e) => setUsername(e.target.value)} required />
                    </div>
                    <div>
                        <label>סיסמה</label>
                        <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
                    </div>
                    <button type="submit" className="submit-button">
                        {isRegister ? 'צור חשבון' : 'התחבר'}
                    </button>
                    {error && <div className="error-message">{error}</div>}
                </form>
                <button onClick={() => setIsRegister(!isRegister)} className="link-button">
                    {isRegister ? 'כבר יש לך חשבון? להתחברות' : 'עוד לא נרשמת? ליצירת חשבון'}
                </button>
            </div>
        </div>
    );
}

export default Login;