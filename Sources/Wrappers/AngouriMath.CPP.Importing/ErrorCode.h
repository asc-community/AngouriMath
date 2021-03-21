
struct ErrorCode
{
public:
    ~ErrorCode();
    bool is_ok() const;
    char* name() const { return _name; }
    char* stack_trace() const { return _stackTrace; }
private:
    char* _name;
    char* _stackTrace;
};
